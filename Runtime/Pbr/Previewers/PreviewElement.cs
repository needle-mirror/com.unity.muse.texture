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

        ActionButton m_EditButton;
        ActionButton m_ActionButton;

        ActionGroup m_FeedbackGroup;
        ActionButton m_LikeButton;
        ActionButton m_DislikeButton;

        VisualElement m_ButtonContainer;
        readonly ActionButton m_BookmarkButton;
        VisualElement m_LeftVerticalContainer;

        protected bool m_IsError =>
            m_PreviewImage?.LoadingState == GenericLoader.State.Error ||
            m_PreviewPbr?.GenericLoader.LoadingState == GenericLoader.State.Error;

        public PreviewType ActivePreviewState => m_ActivePreviewState;
        public UnityEngine.Texture ImagePreview => m_PreviewImage.image;
        public MaterialMapPreview PbrPreview => m_PreviewPbr;
        public event Action<PreviewType> OnPreviewTypeChanged;

        public RotationManipulator RotationManipulator => m_PreviewPbr.RotationManipulator;

        public Material Material => m_PreviewPbr.CurrentMaterial;

        const float k_LeftSideWidthVisible = 107f;
        const float k_EditIconWidthVisible = 70f;

        protected PreviewElement(List<PbrMaterialData> pbrMaterialData, Artifact artifact)
            : base(artifact)
        {
            if (artifact is ImageArtifact imageArtifact)
            {
                m_ActivePreviewState = imageArtifact.IsPbrMode ? PreviewType.PBR : PreviewType.Image;
            }

            EnableInClassList("no-mouse", !Input.mousePresent);

            styleSheets.Add(ResourceManager.Load<StyleSheet>(Common.PackageResources.resultItemStyleSheet));
            m_PreviewImage = new PreviewImage();
            m_PreviewImage.OnSelected += ArtifactSelected;

            m_PreviewPbr = new PreviewPbrArtifact(pbrMaterialData);
            m_PreviewPbr.OnArtifactSelected += ArtifactSelected;

            Add(m_PreviewImage);
            Add(m_PreviewPbr);

            style.flexGrow = 1;
            m_ButtonContainer = new VisualElement();
            m_ButtonContainer.AddToClassList("muse-asset-image__control-buttons-container");

            m_EditButton = new ActionButton { name = "refine", icon = "pen", tooltip = TextContent.refineTooltip };
            m_EditButton.AddToClassList("refine-button");
            m_EditButton.AddToClassList("refine-button-item");
            m_EditButton.clicked += OnRefineClicked;

            m_ActionButton = new ActionButton { name = "more", icon = "ellipsis", tooltip = "More options" };
            m_ActionButton.AddToClassList("refine-button");
            m_ActionButton.AddToClassList("refine-button-item");
            m_ActionButton.clicked += () => OnMenuTriggerClicked();
            m_ActionButton.SetEnabled(true);
            m_ButtonContainer.Add(m_ActionButton);
            m_ButtonContainer.Add(m_EditButton);

            m_LeftVerticalContainer = new VisualElement();
            m_LeftVerticalContainer.AddToClassList("left-vertical-container");
            m_ButtonContainer.Add(m_LeftVerticalContainer);
            m_BookmarkButton = new ActionButton
            {
                tooltip = TextContent.bookmarkButtonTooltip,
                icon = "star"
            };
            m_BookmarkButton.clicked += OnBookmarkClicked;
            m_BookmarkButton.AddToClassList("container-button");

            m_FeedbackGroup = new ActionGroup()
            {
                compact = true,
                justified = false,
                vertical = true,
                selectionType = SelectionType.None,
                style =
                {
                    flexGrow = 0f
                }
            };

            m_FeedbackGroup.AddToClassList("container-button");

            m_LikeButton = new ActionButton()
            {
                name = "LikeBtn",
                tooltip = TextContent.likeTooltip,
                icon = "like"
            };

            m_LikeButton.clicked += OnLikeClicked;

            m_DislikeButton = new ActionButton()
            {
                name = "DislikeBtn",
                tooltip = TextContent.dislikeTooltip,
                icon = "dislike"
            };

            m_DislikeButton.clicked += OnDislikeClicked;

            m_FeedbackGroup.Add(m_LikeButton);
            m_FeedbackGroup.Add(m_DislikeButton);

            m_LeftVerticalContainer.Add(m_BookmarkButton);
            m_LeftVerticalContainer.Add(m_FeedbackGroup);

            UpdateVisuals();
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
        }

        void OnLikeClicked()
        {
            var feedbackManager = CurrentModel.GetData<FeedbackManager>();
            feedbackManager.ToggleLike(m_Artifact);

            UpdateFeedback();
        }

        void OnDislikeClicked()
        {
            var feedbackManager = CurrentModel.GetData<FeedbackManager>();
            feedbackManager.ToggleDislike(m_Artifact);

            UpdateFeedback();
        }

        void UpdateFeedback()
        {
            var feedbackManager = CurrentModel.GetData<FeedbackManager>();
            var isLiked = feedbackManager.IsLiked(m_Artifact);
            m_LikeButton.icon = isLiked ? "like-filled" : "like";

            m_DislikeButton.EnableInClassList("container-hidden", isLiked);

            var isDisliked = feedbackManager.IsDisliked(m_Artifact);
            m_DislikeButton.icon = isDisliked ? "dislike-filled" : "dislike";

            m_LikeButton.EnableInClassList("container-hidden", isDisliked);

        }

        void OnGeometryChangedEvent(GeometryChangedEvent evt)
        {
            UpdateEditButton();
            UpdateLeftSideButtons();
            UpdateBookmark();
        }

        void OnBookmarkClicked()
        {
            var bookmark = CurrentModel.GetData<BookmarkManager>();
            bookmark.Bookmark(m_Artifact, !bookmark.IsBookmarked(m_Artifact));

            UpdateBookmark();
        }

        void UpdateBookmark()
        {
            var isBookmarked = IsBookmarked();
            m_BookmarkButton.EnableInClassList("bookmarked", isBookmarked);
            m_BookmarkButton.icon = isBookmarked ? "star-filled" : "star";
        }

        protected bool IsBookmarked()
        {
            return CurrentModel.GetData<BookmarkManager>().IsBookmarked(m_Artifact);
        }

        internal bool IsDisliked()
        {
            return CurrentModel.GetData<FeedbackManager>().IsDisliked(m_Artifact);
        }

        internal bool IsLiked()
        {
            return CurrentModel.GetData<FeedbackManager>().IsLiked(m_Artifact);
        }

        void UpdateEditButton()
        {
            m_EditButton.EnableInClassList("refine-button-hidden", !ShouldEditButtonBeVisible());
            m_EditButton.EnableInClassList("refine-button", ShouldEditButtonBeVisible());
        }

        void UpdateLeftSideButtons()
        {
            m_LeftVerticalContainer.EnableInClassList("container-hidden", !ShouldLeftSideButtonBeVisible());
        }

        internal bool ShouldLeftSideButtonBeVisible()
        {
            return resolvedStyle.width >= k_LeftSideWidthVisible && resolvedStyle.height >= k_LeftSideWidthVisible;
        }

        protected bool ShouldEditButtonBeVisible()
        {
            return resolvedStyle.width >= k_EditIconWidthVisible;
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
            UpdateFeedback();
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
            m_ButtonContainer.style.display = canRefineBookmark && !m_IsError ? DisplayStyle.Flex : DisplayStyle.None;
            m_ButtonContainer.visible = canRefineBookmark;
            m_EditButton.SetEnabled(m_PreviewImage.image != null);
            m_ActionButton.visible = isArtifactAvailable;
            m_EditButton.visible = canRefine;
        }

        public void SetCurrentState(PreviewType type)
        {
            m_ActivePreviewState = type;
            UpdateVisuals();
            SetPreviewImage(m_Artifact);
            if (m_Artifact is ImageArtifact imageArtifact)
                imageArtifact.IsPbrMode = m_ActivePreviewState == PreviewType.PBR;

            OnPreviewTypeChanged?.Invoke(m_ActivePreviewState);
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

        public GenericLoader.State GetLoadingState()
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