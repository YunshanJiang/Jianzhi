using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;

namespace Starscape.Common
{
    public static class Utils
    {
        public static IEnumerable<ValueDropdownItem<string>> GetAllTags()
        {
#if UNITY_EDITOR
            foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                yield return new ValueDropdownItem<string>(tag, tag);
            }
#else
            yield break;
#endif
        }

        public static string GetDisplayName([CanBeNull] this InputAction _inputAction)
        {
            if (_inputAction == null)
            {
                return string.Empty;
            }

            if (!_inputAction.enabled)
            {
                _inputAction.Enable();
            }

            var control = _inputAction.activeControl;
            if (control != null)
            {
                return control.displayName ?? control.shortDisplayName ?? control.name;
            }

            return _inputAction.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
        }
    }
}
