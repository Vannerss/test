// Scripts/Misc/GrappleTarget.cs
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Grapple Target")]
    [RequireComponent(typeof(Renderer))] // Ensure there's a renderer to change color
    public class GrappleTarget : MonoBehaviour
    {
        public Color highlightColor = Color.cyan;
        private Color m_originalColor;
        private Renderer m_renderer;
        private bool m_isHighlighted = false;

        protected virtual void Start()
        {
            m_renderer = GetComponent<Renderer>();
            if (m_renderer.material != null) // Check if material exists
            {
                m_originalColor = m_renderer.material.color;
            }
            else
            {
                Debug.LogWarning($"GrappleTarget {name} is missing a Renderer or Material for highlighting.", this);
            }
        }

        public void SetHighlight(bool highlighted)
        {
            if (m_isHighlighted == highlighted || m_renderer == null || m_renderer.material == null) return;

            m_isHighlighted = highlighted;
            m_renderer.material.color = highlighted ? highlightColor : m_originalColor;
        }

        // Reset color if the object is disabled while highlighted
        protected virtual void OnDisable()
        {
            SetHighlight(false);
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = highlightColor;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}