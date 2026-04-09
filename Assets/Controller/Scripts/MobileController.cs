using UnityEngine;

public class MobileController : MonoBehaviour
{
    [SerializeField]
    private bool m_isMobileTest;
    public bool IsMobile => Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer || m_isMobileTest;
    [SerializeField]
    private GameObject m_root;
    [SerializeField]
    private MobileJoystick m_mobileJoystick;
    public MobileJoystick MobileJoystick => m_mobileJoystick;
    [SerializeField]
    private MobileCameraRotate m_mobileCameraRotate;
    public MobileCameraRotate MobileCameraRotate => m_mobileCameraRotate;

    private void Awake()
    {
        if(m_root) m_root.SetActive(IsMobile);
    }
}
