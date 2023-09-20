using System;
using System.Collections.Generic;
using Unity.Muse.Common;
using Unity.Muse.Texture.Analytics;
using UnityEngine;

namespace Unity.Muse.Texture
{
    [Serializable]
    public sealed class ImageArtifact : Artifact<Texture2D>, IGenerateArtifact, IVariateArtifact, IInpaintArtifact, IUpscaleArtifact
    {
        public override string FileExtension => "png";

        internal bool IsPbrMode = false;

        /// <summary>
        /// Material meta data for the generated material
        /// </summary>
        public MaterialData MaterialMetaData;

        public ImageArtifact(string guid, uint seed)
            : base(guid, seed) { }

        public ImageArtifact()
            : base(string.Empty, 0) { }


        /// <inheritdoc cref="Artifact{T}"/>
        public override Texture2D ConstructFromData(byte[] data)
        {
            Texture2D tex = TextureUtils.Create();
            tex.LoadImage(data);

            return tex;
        }

        /// <inheritdoc cref="Artifact{T}"/>
        protected override Texture2D CreateFromData(byte[] data, bool updateCache)
        {
            Texture2D tex = ConstructFromData(data);

            if (updateCache)
            {
                WriteToCache(data);
            }

            return tex;
        }

        /// <inheritdoc cref="Artifact{T}"/>
        protected override Texture2D ReadFromCache(out byte[] rawData)
        {
            if (ArtifactCache.Read(this) is Texture2D tex)
            {
                rawData = ArtifactCache.ReadRawData(this);
                return tex;
            }
            rawData = null;
            return null;
            /*
        if (!GeneratedArtifactCache.TryGetCachedArtifact(this, out var tex, out rawData))
        {
            rawData = null;
            return null;
        }

        return tex;*/
        }

        protected override byte[] ReadFromCacheRaw()
        {
            ReadFromCache(out var raw);
            return raw;
        }

        /// <inheritdoc cref="Artifact{T}"/>
        protected override void WriteToCache(byte[] value)
        {
            ArtifactCache.Write(this, value);

            // TODO: Cloudlab
            //GeneratedArtifactCache.WriteArtifactToCache(this, bytes);
        }

        public override void GetPreview(ArtifactPreviewDelegate onDoneCallback, bool useCache)
        {
            GetArtifact((instance, data, message) =>
            {
                onDoneCallback?.Invoke(instance, data, message);
            }, true);
        }

        /// <summary>
        /// Only called once for a generation group
        /// </summary>
        /// <param name="model">Model used.</param>
        public override void StartGenerate(Model model)
        {
            try
            {
                Model.SendAnalytics(new GenerateAnalyticsData
                {
                    prompt = this.GetOperator<PromptOperator>()?.GetPrompt(),
                    prompt_negative = this.GetOperator<NegativePromptOperator>()?.GetNegativePrompt(),
                    inpainting_used = this.GetOperator<MaskOperator>()?.Enabled() ?? false,
                    images_generated_nr = m_Operators.GetOperator<GenerateOperator>()?.GetCount() ?? 0,
                    reference_image_used = m_Operators.GetOperator<ReferenceOperator>()?.Enabled() ?? false,
                    is_variation = m_Operators.GetOperator<ReferenceOperator>()?.Enabled() ?? false,
                });
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public override void Generate(Model model)
        {
            byte[] buffer = new byte[4];
            var random = new System.Random();

            random.NextBytes(buffer);
            var maskOperator = m_Operators.GetOperator<MaskOperator>();
            var promptOperator = m_Operators.GetOperator<PromptOperator>();
            var prompt = promptOperator?.GetPrompt();
            var negativePromptOperator = m_Operators.GetOperator<NegativePromptOperator>();


            //var loraOperator = m_Operators.FirstOrDefault(x => x.GetType() == typeof(LoraOperator)) as LoraOperator;
            //we will pass the loraOperator guid to settings when it's possible
            var settings = new TextToImageRequest(negativePromptOperator?.GetNegativePrompt(), true, (uint)BitConverter.ToUInt32(buffer, 0), (int)GenerativeAIBackend.GeneratorModel.StableDiffusionV_1_4, 512, 512);

            if (maskOperator != null && maskOperator.Enabled())
            {
                if (model.SelectedArtifact is not IInpaintArtifact inpaintArtifact) return;

                settings.seamless = maskOperator.GetSeamless();
                var parentGuid = model.SelectedArtifact.Guid;

                inpaintArtifact.GenerateInpaint(prompt, parentGuid, maskOperator.GetMask(), MaskType.UserDefined, settings, OnGeneratingDone);
            }
            else
            {
                var generateArtifact = model.SelectedArtifact ?? new ImageArtifact();

                if (generateArtifact is not IGenerateArtifact generateAbleArtifact) return;

                generateAbleArtifact.Generate(prompt, settings, OnGeneratingDone);
            }
        }

        public override void RetryGenerate(Model model)
        {
            var referenceOp = m_Operators.GetOperator<ReferenceOperator>();
            var isVariation = referenceOp != null && referenceOp.Enabled();

            if(isVariation)
                Variate(m_Operators);
            else
                Generate(model);
        }

        public override void Variate(List<IOperator> ops)
        {
            byte[] buffer = new byte[4];
            var random = new System.Random();
            random.NextBytes(buffer);
            var promptOperator = ops.GetOperator<PromptOperator>();
            var negativePromptOperator = ops.GetOperator<NegativePromptOperator>();
            var settings = new TextToImageRequest(negativePromptOperator?.GetNegativePrompt(), true, (uint)BitConverter.ToUInt32(buffer, 0), (int)GenerativeAIBackend.GeneratorModel.StableDiffusionV_1_4, 512, 512);
            var referenceOp = ops.GetOperator<ReferenceOperator>();
            SetOperators(ops);
            GenerativeAIBackend.VariateImage(referenceOp.GetOperatorData().settings[0], promptOperator?.GetPrompt(), settings, OnGeneratingDone);

        }

        public void Variate(Model model, int variationNbr = 4)
        {
            byte[] buffer = new byte[4];
            var random = new System.Random();

            var promptOperator = m_Operators.GetOperator<PromptOperator>();
            var negativePromptOperator = m_Operators.GetOperator<NegativePromptOperator>();

            for (var i = 0; i < variationNbr; ++i)
            {
                random.NextBytes(buffer);
                var settings = new TextToImageRequest(negativePromptOperator?.GetNegativePrompt(), true, (uint)BitConverter.ToUInt32(buffer, 0), (int)GenerativeAIBackend.GeneratorModel.StableDiffusionV_1_4, 512, 512);

                var newArtifact = new ImageArtifact();
                newArtifact.SetOperators(m_Operators);
                GenerativeAIBackend.VariateImage(Guid, promptOperator?.GetPrompt(), settings, newArtifact.OnGeneratingDone);
                model.AddAsset(newArtifact);
            }
        }

        public void Upscale(Model model)
        {
            if (this.GetOperator<UpscaleOperator>() != null)
                return;

            var ops = new List<IOperator>();

            var upscaleOp = new UpscaleOperator();
            upscaleOp.Enable(true);
            ops.Add(upscaleOp);

            var promptOp = this.GetOperator<PromptOperator>();
            var jsonOp = JsonUtility.ToJson(promptOp); //We do a deep copy
            promptOp = JsonUtility.FromJson<PromptOperator>(jsonOp);
            promptOp.Enable(false);

            ops.Add(promptOp);

            var newArtifact = new ImageArtifact();
            newArtifact.SetOperators(ops);
            model.AddAsset(newArtifact);

            MuseTextureBackend.UpscaleImage(this, newArtifact.OnGeneratingDone);
        }

        void OnGeneratingDone(GuidResponse response, string error)
        {
            if (response != null)
            {
                Guid = response.guid;
                Seed = response.seed;
            }

            OnGenerationDone?.Invoke(this, error);
        }

        /// <summary>
        /// Creating a new MaterialRefineView
        /// </summary>
        /// <returns>new Instance of MaterialRefineView</returns>
        public override ArtifactView CreateCanvasView()
        {
            var view = new MaterialRefineView(this);
            view.UpdateView();
            return view;
        }

        public override ArtifactView CreateView()
        {
            return new ResultItemVisualElement(this);
        }

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        public static void RegisterArtifact()
        {
            ArtifactFactory.SetArtifactTypeForMode<ImageArtifact>("TextToImage");
        }

        public void Generate(string prompt, TextToImageRequest settings, Action<TextToImageResponse, string> onDone)
        {
            MuseTextureBackend.GenerateImage(prompt, settings, onDone);
        }

        public void GenerateInpaint(string prompt,
            string sourceGuid,
            Texture2D mask,
            MaskType maskType,
            TextToImageRequest settings,
            Action<TextToImageResponse, string> onDone)
        {
            GenerativeAIBackend.GenerateInpainting(prompt, sourceGuid, mask, maskType, settings, onDone);
        }

        /// <summary>
        /// Meta data associated with Generated Materials
        /// </summary>
        public class MaterialData
        {
            /// <summary>
            /// Height value for the material
            /// </summary>
            public float height;
            /// <summary>
            /// Metallic value for the material
            /// </summary>
            public float metallic;
            /// <summary>
            /// roughness value for the material
            /// </summary>
            public float roughness;

            /// <summary>
            /// We don't want to initialize the data by default aka when serialization occurs
            /// </summary>
            /// <param name="initializeData"></param>
            public MaterialData(bool initializeData = false)
            {
                if(!initializeData) return;

                var shader = MaterialGeneratorUtils.GetDefaultShaderForPipeline();
                height = shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex("_HeightIntensity"));
                metallic = shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex("_MetallicIntensity"));
                roughness = shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex("_RoughnessIntensity"));
            }
        }
    }
}
