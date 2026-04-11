using Starscape.Simulation;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerController PlayerController => m_playerController;
     public bool IsInLab { get; private set; }
    [SerializeField]
    private PlayerController m_playerController;

    private void OnValidate()
    {
        if (m_playerController == null)
        {
            m_playerController = GetComponent<PlayerController>();
        }
    }

    private void Awake()
    {
        if (m_playerController == null)
        {
            m_playerController = GetComponent<PlayerController>();
        }
    }

    private void Start()
    {
        GameManager.Instance.SetPlayer(this);
    }

      public void SetIsInLab(bool _isInLab)
    {
        IsInLab = _isInLab;
    }
}
