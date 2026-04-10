using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 图片查看器
    /// </summary>
    public class ImageViewerSceneStep : ViewerBaseSceneStep
    {
        [Serializable]
        private struct PictureData
        {
            public string Title;
            public Sprite Sprite;
        }
        [TitleGroup("Image Viewer")]
        [SerializeField]
        private List<PictureData> m_pictureDataSet;
        [TitleGroup("Image Viewer")][ValueDropdown("GetTitleDropdownList")]
        [SerializeField]
        private int m_firstPictureIndex;
        public override bool IsNeedFlipButtons => true;
        protected override int ViewerCount => m_pictureDataSet.Count;

        protected override void OnStepStart()
        {
            base.OnStepStart();
            CurrentIndex = m_firstPictureIndex;
            GameManager.Instance.UIManager.GetImageViewerSprite += GetSprite;
            GameManager.Instance.UIManager.ViewerDisplay(GetViewerTitle(), this, GetViewerTitleSet());
        }

        protected override void OnStepReset()
        {
            base.OnStepReset();
            CurrentIndex = m_firstPictureIndex;
            GameManager.Instance.UIManager.ViewerDisplay(GetViewerTitle(), this, GetViewerTitleSet());
        }

        protected override void OnStepResume()
        {
            base.OnStepResume();
            if (CurrentIndex == -1)
            {
                CurrentIndex = m_firstPictureIndex;
            }
            GameManager.Instance.UIManager.GetImageViewerSprite += GetSprite;
            GameManager.Instance.UIManager.ViewerDisplay(GetViewerTitle(), this, GetViewerTitleSet());
        }

        protected override void OnStepEnd()
        {
            base.OnStepEnd();
            GameManager.Instance.UIManager.GetImageViewerSprite -= GetSprite;
        }

        protected override void OnClose()
        {
            base.OnClose();
            StepEnd();
        }

        private (Sprite Sprite, string Title) GetSprite()
        {
            var data = m_pictureDataSet[CurrentIndex];
            return (data.Sprite, data.Title);
        }

        private IEnumerable<ValueDropdownItem<int>> GetTitleDropdownList()
        {
            for (var index = 0; index < m_pictureDataSet.Count; index++)
            {
                var item = m_pictureDataSet[index];
                yield return new ValueDropdownItem<int>(item.Title, index);
            }
        }
    }
}
