using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.Muse.Texture
{
    internal abstract class PreviewElement : ArtifactView, IDisposable
    {
        public bool MaterialPreviewEnabled => m_PreviewPbr.PreviewEnabled;
        public event Action<Artifact> OnSelected;

        protected PreviewImage m_PreviewImage;
        protected PreviewPbrArtifact m_PreviewPbr;

        protected PreviewType m_ActivePreviewState = PreviewType.Image;

        private ActionButton m_EditButton;
        private ActionButton m_ActionButton;
        private VisualElement m_ButtonContainer;
        readonly ActionButton m_BookmarkButton;

        public PreviewType ActivePreviewState => m_ActivePreviewState;
        public UnityEngine.Texture ImagePreview => m_PreviewImage.image;
        public MaterialMapPreview PbrPreview => m_PreviewPbr;
        public event Action<PreviewType> OnPreviewTypeChanged;

        public RotationManipulator RotationManipulator => m_PreviewPbr.RotationManipulator;

        public Material Material => m_PreviewPbr.CurrentMaterial;


        protected PreviewElement(List<PbrMaterialData> pbrMaterialData, Artifact artifact)
            : base(artifact)
        {
            EnableInClassList("no-mouse", !Input.mousePresent);

            styleSheets.Add(Resources.Load<StyleSheet>("uss/Bookmark"));
            m_PreviewImage = new PreviewImage();
            m_PreviewImage.OnSelected += ArtifactSelected;

            m_PreviewPbr = new PreviewPbrArtifact(pbrMaterialData);
            m_PreviewPbr.OnArtifactSelected += ArtifactSelected;

            Add(m_PreviewImage);
            Add(m_PreviewPbr);

            style.flexGrow = 1;
            m_ButtonContainer = new VisualElement();
            m_ButtonContainer.AddToClassList("muse-asset-image__control-buttons-container");

            m_EditButton = new ActionButton { name="refine", icon = "pen", tooltip = "Refine image" };
            m_EditButton.AddToClassList("refine-button");
            m_EditButton.AddToClassList("refine-button-item");
            m_EditButton.clicked += OnRefineClicked;

            m_ActionButton = new ActionButton { name="more", icon = "ellipsis", tooltip = "More options" };
            m_ActionButton.AddToClassList("refine-button");
            m_ActionButton.AddToClassList("refine-button-item");
            m_ActionButton.clicked += () => OnMenuTriggerClicked();
            m_ActionButton.SetEnabled(true);

            m_ButtonContainer.Add(m_ActionButton);
            m_ButtonContainer.Add(m_EditButton);

            m_BookmarkButton = new ActionButton();
            m_BookmarkButton.clicked += OnBookmarkClicked;
            m_BookmarkButton.icon = "star";
            m_BookmarkButton.AddToClassList("bookmark-button");
            m_BookmarkButton.AddToClassList("refine-button");
            m_ButtonContainer.Add(m_BookmarkButton);

            UpdateVisuals();
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        void OnBookmarkClicked()
        {
            var bookmark = CurrentModel.GetData<BookmarkManager>();
            bookmark.Bookmark(m_Artifact, !bookmark.IsBookmarked(m_Artifact));

            UpdateBookmark();
        }

        void UpdateBookmark()
        {
            var isBookmarked = CurrentModel.GetData<BookmarkManager>().IsBookmarked(m_Artifact);
            m_BookmarkButton.EnableInClassList("refine-button", !isBookmarked);
            m_BookmarkButton.icon = isBookmarked ? "star-filled" : "star";
        }

        void OnMenuTriggerClicked()
        {
            m_ButtonContainer.AddToClassList("is-hovered");
            OnActionMenu(m_ActionButton);
        }

        protected override void MenuDismissed()
        {
            m_ButtonContainer.RemoveFromClassList("is-hovered");
        }

        void ArtifactSelected(Artifact artifact)
        {
            OnSelected?.Invoke(artifact);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            UpdateVisuals();
            UpdateView();
            UpdateBookmark();
        }

        void UpdateVisuals()
        {
            m_ButtonContainer.RemoveFromHierarchy();

            var enablePreviewImage = m_ActivePreviewState == PreviewType.Image;
            var enablePreviewPbr = m_ActivePreviewState == PreviewType.PBR;

            m_PreviewImage.style.display =
                enablePreviewImage ? DisplayStyle.Flex : DisplayStyle.None;
            m_PreviewPbr.style.display =
                enablePreviewPbr ? DisplayStyle.Flex : DisplayStyle.None;

            VisualElement visualElementFront = enablePreviewImage ? m_PreviewImage : m_PreviewPbr;
            visualElementFront.BringToFront();
            visualElementFront =
                enablePreviewImage ? m_PreviewImage : m_PreviewPbr.GetChildren<VisualElement>(false).First();

            visualElementFront.Add(m_ButtonContainer);
            m_PreviewImage.OnLoadedPreview += UpdateView;
            m_PreviewPbr.GenericLoader.OnLoadingStateChanged += state =>
            {
                if (state == GenericLoader.State.None)
                    UpdateView();
            };
        }

        bool isArtifactAvailable
        {
            get
            {
                return ActivePreviewState switch
                {
                    PreviewType.PBR => m_PreviewPbr.CurrentMaterial != null,
                    PreviewType.Image => m_PreviewImage.image != null,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        protected bool canRefine => isArtifactAvailable &&
                                    CurrentModel != null && !CurrentModel.isRefineMode;

        protected bool canRefineBookmark => isArtifactAvailable &&
            CurrentModel != null;

        public override void UpdateView()
        {
            m_ButtonContainer.style.display = canRefineBookmark ? DisplayStyle.Flex : DisplayStyle.None;
            m_ButtonContainer.visible = canRefineBookmark;
            m_EditButton.SetEnabled(m_PreviewImage.image != null);
            m_ActionButton.visible = canRefine;
            m_EditButton.visible = canRefine;
        }

        public void SetCurrentState(PreviewType type)
        {
            m_ActivePreviewState = type;
            UpdateVisuals();
            SetPreviewImage(m_Artifact);
            OnPreviewTypeChanged?.Invoke(m_ActivePreviewState);
            if(m_Artifact is ImageArtifact imageArtifact)
                imageArtifact.IsPbrMode = m_ActivePreviewState == PreviewType.PBR;
        }

        protected void SetPreviewImage(Artifact image)
        {
            m_Artifact = image;

            switch (m_ActivePreviewState)
            {
                case PreviewType.Image:
                    m_PreviewPbr.Disable();
                    m_PreviewImage.SetAsset(image);
                    break;
                case PreviewType.PBR:
                    m_PreviewPbr.Enable();
                    m_PreviewPbr.SetAsset(image);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Dispose()
        {
            m_PreviewPbr?.Dispose();
        }

        public GenericLoader.State  GetLoadingState()
        {
            return m_ActivePreviewState switch
            {
                PreviewType.Image => m_PreviewImage.LoadingState,
                PreviewType.PBR => m_PreviewPbr.LoadingState,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}