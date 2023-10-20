using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.UIElements;
using GridView = Unity.Muse.Common.GridView;

namespace Unity.Muse.Texture
{
    internal class MaterialRefineView : ArtifactView
    {
        private Image m_Preview;
        private ResultItemVisualElement m_TargetVe;
        private GenericLoader m_GenericLoader;

        private RotationProxy m_RotationManipulator;
        private MaterialPreviewSelector m_MaterialPreviewSelector;
        private MaterialPreviewSettings m_MaterialPreviewSettings;
        private MaterialInspectorView m_MaterialInspectorView;

        private ActionButton m_CloseButton;

        private NodesList m_NodesList;
        private VisualElement m_Canvas;

        private VisualElement m_ControlContent;

        AssetsList m_AssetsList;
        private GridView m_GridView;

        public MaterialRefineView(Artifact artifact) : base(artifact)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("MaterialInspector"));

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }


        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_Canvas = evt.destinationPanel.visualTree.Q("Canvas");
            m_ControlContent = new VisualElement()
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    flexGrow = 1f
                }
            };

            m_Canvas.Add(m_ControlContent);

            m_MaterialPreviewSelector = new MaterialPreviewSelector();
            m_ControlContent.Add(m_MaterialPreviewSelector);

            m_ControlContent.Add(new VisualElement()
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    flexGrow = 1f
                }
            });

            m_MaterialPreviewSettings = new MaterialPreviewSettings();
            m_ControlContent.Add(m_MaterialPreviewSettings);

            m_Preview = new Image();
            Add(m_Preview);

            m_GenericLoader = new GenericLoader(GenericLoader.State.Loading)
            {
                style =
                {
                    backgroundColor = new Color(0.1568628f, 0.1568628f, 0.1568628f, 1f),
                    position = Position.Absolute,
                    width = Length.Percent(100),
                    height = Length.Percent(100)
                }
            };

            Add(m_GenericLoader);

            m_MaterialInspectorView = new MaterialInspectorView();
            m_NodesList = evt.destinationPanel.visualTree.Q<NodesList>();

            m_NodesList.parent.Add(m_MaterialInspectorView);


            m_CloseButton = new ActionButton()
            {
                name = "CloseButton",
                icon = "caret-left",
                label = "Generations",
            };

            m_CloseButton.AddToClassList("muse-material-inspector--close-button");

            m_MaterialInspectorView.hierarchy.Add(m_CloseButton);

            parent.style.backgroundColor = new Color(0f, 0f, 0f, 0f); //Removing the Background of the Node

            m_AssetsList = panel.visualTree.Q<AssetsList>();
            m_GridView = m_AssetsList.Q<GridView>();
            m_GridView.OnSelectionRefreshed += OnSelectionRefreshed;

            RegisterTarget();

            m_MaterialPreviewSelector.OnPreviewSelected += OnPreviewSelected;
            m_MaterialPreviewSettings.OnTargetPrimitiveChanged += OnTargetPrimitiveChanged;
            m_MaterialPreviewSettings.OnHdriChanged += OnHdriChanged;
            m_MaterialInspectorView.OnMaterialPropertiesChanged += OnMaterialPropertiesChanged;

            CurrentModel.OnActiveToolChanged += OnActiveToolChanged;

            m_CloseButton.clickable.clicked += OnCloseButtonClicked;

            UpdateView();
        }


        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_GridView.OnSelectionRefreshed -= OnSelectionRefreshed;
            m_MaterialPreviewSelector.OnPreviewSelected -= OnPreviewSelected;
            m_MaterialPreviewSettings.OnTargetPrimitiveChanged -= OnTargetPrimitiveChanged;
            m_MaterialPreviewSettings.OnHdriChanged -= OnHdriChanged;
            m_MaterialInspectorView.OnMaterialPropertiesChanged -= OnMaterialPropertiesChanged;

            m_CloseButton.clickable.clicked -= OnCloseButtonClicked;
            m_MaterialInspectorView.hierarchy.Remove(m_CloseButton);
            m_CloseButton = null;

            if (CurrentModel is not null)
                CurrentModel.OnActiveToolChanged -= OnActiveToolChanged;

            m_AssetsList = null;
            m_GridView = null;

            UnRegisterTarget();

            m_NodesList.parent.Remove(m_MaterialInspectorView);
            m_MaterialInspectorView = null;

            m_NodesList.style.display = DisplayStyle.Flex;
            m_NodesList = null;

            m_ControlContent.Clear();
            m_Canvas.Remove(m_ControlContent);

            m_MaterialPreviewSelector = null;
            m_MaterialPreviewSettings = null;

            Remove(m_Preview);
            m_Preview = null;

            Remove(m_GenericLoader);
            m_GenericLoader = null;
        }

        private void OnSelectionRefreshed(IEnumerable<int> index)
        {
            // Need to delay a frame since when gridview has a new selection, the related ArtifactView is not yet added to its children
            schedule.Execute(() =>
            {
                UnRegisterTarget();
                RegisterTarget();
                UpdateView();
            }).ExecuteLater(1);
        }

        void RegisterTarget()
        {
            m_TargetVe = (ResultItemVisualElement)m_AssetsList.GetView((Artifact)m_GridView.selectedItems.FirstOrDefault());

            if (m_TargetVe == null)
                return;

            m_TargetVe.OnPreviewTypeChanged += OnPreviewTypeChanged;
            m_TargetVe.OnLoadingStateChanged += OnTargetLoadingStateChanged;

            m_RotationManipulator = new RotationProxy(m_TargetVe.RotationManipulator);
            this.AddManipulator(m_RotationManipulator);
        }

        void UnRegisterTarget()
        {
            if (m_TargetVe != null)
            {
                m_TargetVe.OnLoadingStateChanged -= OnTargetLoadingStateChanged;
                m_TargetVe.OnPreviewTypeChanged -= OnPreviewTypeChanged;
                m_TargetVe = null;
            }


            if (m_RotationManipulator != null)
            {
                this.RemoveManipulator(m_RotationManipulator);
                m_RotationManipulator = null;
            }
        }

        private void OnTargetLoadingStateChanged(GenericLoader.State obj)
        {
            UpdateView();
        }

        private void OnMaterialPropertiesChanged()
        {
            m_TargetVe.PbrPreview.RefreshRender();

            if (m_TargetVe.Artifact is not ImageArtifact imageArtifact) return;

            if (imageArtifact.MaterialMetaData is not { Initialized: true })
            {
                imageArtifact.MaterialMetaData = new ImageArtifact.MaterialData(true);
            }
            imageArtifact.MaterialMetaData.GetValuesFromMaterial(m_TargetVe.Material);
        }

        private void OnPreviewSelected(MaterialPreviewItem obj)
        {
            if ((obj == MaterialPreviewItem.Artifact && m_TargetVe?.ActivePreviewState != PreviewType.Image) ||
                (obj != MaterialPreviewItem.Artifact && m_TargetVe?.ActivePreviewState != PreviewType.PBR))
            {
                m_TargetVe?.PerformAction((int)Actions.SwitchPreview, new ActionContext(), null);
            }

            UpdateView();
        }


        private void OnPreviewTypeChanged(PreviewType obj)
        {
            UpdateView();
        }

        public override void UpdateView()
        {
            if (m_TargetVe == null)
            {
                SetImageView();
                return;
            }

            m_GenericLoader.style.display = m_TargetVe.GetLoadingState() == GenericLoader.State.Loading || m_TargetVe.GetLoadingState() == GenericLoader.State.Error
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            m_Preview.style.display = m_GenericLoader.style.display == DisplayStyle.Flex
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            switch (m_TargetVe.ActivePreviewState)
            {
                case PreviewType.PBR:

                    tooltip = "Click + Drag to rotate the model.";

                    m_MaterialPreviewSettings.style.display = m_TargetVe.GetLoadingState() == GenericLoader.State.None
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                    m_MaterialInspectorView.style.display = m_TargetVe.GetLoadingState() == GenericLoader.State.None
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                    m_NodesList.style.display = m_TargetVe.GetLoadingState() == GenericLoader.State.None
                        ? DisplayStyle.None
                        : DisplayStyle.Flex;

                    m_MaterialPreviewSelector.SetMaterial(m_TargetVe.Material);
                    m_MaterialPreviewSettings.SetMaterial(m_TargetVe.Material);
                    m_MaterialInspectorView.SetMaterial(m_TargetVe.Material);

                    m_MaterialPreviewSettings.SelectPrimitive(m_TargetVe.PbrPreview.CurrentPreviewType);
                    m_MaterialPreviewSettings.SelectHdri(m_TargetVe.PbrPreview.CurrentHdriEnvironment);


                    m_Preview.image = m_MaterialPreviewSelector.SelectedPreviewItem switch
                    {
                        MaterialPreviewItem.Material => m_TargetVe.PbrPreview.previewImage.image,
                        MaterialPreviewItem.Artifact => RenderMap(MuseMaterialProperties.baseMapKey),
                        MaterialPreviewItem.BaseMap => RenderMap(MuseMaterialProperties.baseMapKey),
                        MaterialPreviewItem.NormalMap => RenderMap(MuseMaterialProperties.normalMapKey),
                        MaterialPreviewItem.MetallicMap => RenderMap(MuseMaterialProperties.metallicMapKey),
                        MaterialPreviewItem.SmoothnessMap => RenderMap(MuseMaterialProperties.smoothnessMapKey),
                        MaterialPreviewItem.HeightMap => RenderMap(MuseMaterialProperties.heightMapKey),
                        MaterialPreviewItem.AOMap => RenderMap(MuseMaterialProperties.ambientOcclusionMapKey),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    break;

                case PreviewType.Image:
                    SetImageView();
                    m_Preview.image = m_TargetVe.ImagePreview;
                    m_MaterialPreviewSelector.SelectItem(MaterialPreviewItem.Artifact, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CurrentModel.UpdateToolState();
        }

        void SetImageView()
        {
            if (m_MaterialPreviewSelector is null)
                return;

            tooltip = string.Empty;

            m_MaterialPreviewSettings.style.display = DisplayStyle.None;
            m_MaterialInspectorView.style.display = DisplayStyle.None;
            m_NodesList.style.display = DisplayStyle.Flex;
        }

        UnityEngine.Texture RenderMap(int propertyId)
        {
            return m_TargetVe.Material != null ? m_TargetVe.Material.GetTexture(propertyId) : null;
        }

        private void OnHdriChanged(HdriEnvironment environment)
        {
            m_TargetVe.PbrPreview.SetHdriEnvironment(environment);
        }

        private void OnTargetPrimitiveChanged(PrimitiveObjectTypes obj)
        {
            m_TargetVe.PbrPreview.SetPreviewType(obj);
        }

        private void OnActiveToolChanged(ICanvasTool obj)
        {
            if (obj == null || m_TargetVe?.ActivePreviewState == PreviewType.Image) return;

            m_TargetVe?.SetCurrentState(PreviewType.Image);
            UpdateView();
        }

        private void OnCloseButtonClicked()
        {
            CurrentModel.FinishRefineArtifact();
        }

        public override UnityEngine.Texture Preview => m_Preview.image;

        public override VisualElement PaintSurfaceElement => m_Preview;
    }
}