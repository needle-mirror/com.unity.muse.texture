using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Unity.Muse.Common;
using UnityEngine.Networking;

namespace Unity.Muse.Texture
{
    public abstract class MuseTextureBackend: GenerativeAIBackend
    {
        static readonly string k_GenerateBatchPbrMapURL = $"{k_ServiceBaseURL}/pbr/batch/generate";
        static readonly string k_GenerateImageURL = $"{k_TextToImageServiceBaseURL}/generate";
        static readonly string k_UpscaleGeneratedImageURL = $"{k_TextToImageServiceBaseURL}/upscale_image";
        static readonly string k_GenerateDiffuse = $"{k_ServiceBaseURL}/pbr_delighting/generate";

        /// <summary>
        /// Initiate Text to Texture generation on Cloud. It only allocates texture ids and actual generation occurs in background.
        /// Use `RequestStatus` to query progress and `DownloadImage` to download intermediate or final result.
        /// </summary>
        /// <param name="prompt">Text prompt for texture generation</param>
        /// <param name="settings">Additional request settings</param>
        /// <param name="onDone">Callback called when results are received. Callback parameters (TextToImageResponse, string).
        /// <returns>The the reference to the async operation this generates so that it may be cancelled</returns>
        /// In case error occured error string is non-null and other parameters are null</param>
        public static UnityWebRequestAsyncOperation GenerateImage(string prompt,
            TextToImageRequest settings,
            Action<TextToImageResponse, string> onDone)
        {
            void HandleRequest(object data, string error)
            {
                if (onDone != null)
                {
                    if (data != null)
                    {
                        try
                        {
                            var res = JsonUtility.FromJson<TextToImageResponse>(Encoding.UTF8.GetString((byte[])data));
                            res.seed = settings.seed;
                            onDone(res, error);
                            return;
                        }
                        catch (ArgumentException e)
                        {
                            onDone(null, e.Message);
                            return;
                        }

                    }
                    onDone(null, error);
                }
            }

            return SendJSONRequest(k_GenerateImageURL, new TextToImageItemRequest(prompt, settings, AccessToken),
                HandleRequest);
        }

        /// <summary>
        /// Take an artifact and generate a diffuse map from it.
        /// </summary>
        /// <param name="fromImage">base image</param>
        /// <param name="onDone">guid result</param>
        /// <returns></returns>
        public static UnityWebRequestAsyncOperation GenerateDiffuseMap(ImageArtifact fromImage, Action<GuidResponse, string> onDone)
        {
            var request = new DiffuseMapRequest(fromImage.Guid, AccessToken);
            void HandleRequest(object data, string error)
            {
                if (onDone == null) return;
                
                if (data != null && string.IsNullOrEmpty(error))
                {
                    var res = JsonUtility.FromJson<GuidResponse>(Encoding.UTF8.GetString((byte[])data));
                    onDone(res, error);
                    return;
                }
                onDone(null, error);
            }
            
            return SendJSONRequest(k_GenerateDiffuse, request, HandleRequest);
        }

        /// <summary>
        /// Takes an image artifact and requests a PBR map to be generated from it. Must have one call made for each map type you wish to extract.
        /// </summary>
        /// <param name="fromImage">The image artifact to send for PBR map extraction. When requesting a normal map, the heightmap must be used and not the source image artifact</param>
        /// <param name="mapTypes"></param>
        /// <param name="onDone"></param>
        /// <returns>The the reference to the async operation this generates so that it may be cancelled</returns>
        public static UnityWebRequestAsyncOperation GenerateBatchPbrMap(ImageArtifact fromImage, PBRMapTypes[] mapTypes, Action<BatchPBRResponse, string> onDone)
        {
            var backendMapTypeNames = mapTypes.Select(GetMapTypeName).ToArray();

            var request = new GenerateBatchPBRMapRequest(fromImage.Guid, backendMapTypeNames, AccessToken);
            
            void HandleRequest(object data, string error)
            {
                if (onDone != null)
                {
                    if (data != null && String.IsNullOrEmpty(error))
                    {
                        var res = JsonUtility.FromJson<BatchPBRResponse>(Encoding.UTF8.GetString((byte[])data));
                        onDone(res, error);
                        return;
                    }
                    onDone(null, error);
                }
            }
            
            return SendJSONRequest(k_GenerateBatchPbrMapURL, request, HandleRequest);
        }

        internal static string GetMapTypeName(PBRMapTypes mapType) =>
            mapType switch
            {
                PBRMapTypes.BaseMap => "emission",
                PBRMapTypes.Emission => "emission",
                PBRMapTypes.Height => "height",
                PBRMapTypes.Normal => "normal",
                PBRMapTypes.Metallic => "metallic",
                PBRMapTypes.Roughness => "roughness",
                PBRMapTypes.AO => "ao",
                _ => string.Empty
            };

        internal static string GetPBRMapGuid(PBRMapGuids guids, PBRMapTypes mapType) =>
            mapType switch
            {
                PBRMapTypes.BaseMap => guids.emission,
                PBRMapTypes.Emission => guids.emission,
                PBRMapTypes.Height => guids.height,
                PBRMapTypes.Normal => guids.normal,
                PBRMapTypes.Metallic => guids.metallic,
                PBRMapTypes.Roughness => guids.roughness,
                PBRMapTypes.AO => guids.ao,
                _ => string.Empty
            };

        /// <summary>
        /// Request image upscale
        /// </summary>
        /// <param name="artifact">Previously generated texture artifact</param>
        /// <param name="onDone">Callback called when results are received. Callback parameters (UpscaleImageResponse, string).
        /// <returns>The the reference to the async operation this generates so that it may be cancelled</returns>
        /// UpscaleImageResponse contains newly generated image guid, you should track it's progress via RequestStatus call.
        /// In case error occured error string is non-null and other parameters are null</param>
        public static UnityWebRequestAsyncOperation UpscaleImage(Artifact artifact, Action<UpscaleImageResponse, string> onDone)
        {
            void HandleRequest(object data, string error)
            {
                if (onDone != null && onDone.Target != null)
                {
                    if (data != null)
                    {
                        var json = Encoding.UTF8.GetString((byte[]) data);
                        var res = JsonUtility.FromJson<UpscaleImageResponse>(json);
                        onDone(res, error);
                        return;
                    }
                    onDone(null, error);
                }
            }

            return SendJSONRequest(k_UpscaleGeneratedImageURL, new UpscaleImageRequest(artifact.Guid, AccessToken), HandleRequest);
        }
    }
}
