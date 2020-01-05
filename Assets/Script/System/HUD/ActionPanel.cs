using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace Reborn
{
    [UsedImplicitly]
    internal class ActionPanel : MonoBehaviour
    {
        [SerializeField] Collider[] _buttons;

        internal void Close()
        {
            StopCoroutine(nameof(PlayAnimation));
            StartCoroutine(nameof(PlayAnimation), true);
        }
        internal void Open()
        {
            StopCoroutine(nameof(PlayAnimation));
            StartCoroutine(nameof(PlayAnimation), false);
        }
        IEnumerator PlayAnimation(bool isBackward)
        {
            var sprite = GetComponent<UISprite>();
            var visiblePosition = new Vector3(-0, transform.localPosition.y, transform.localPosition.z);
            var hiddenPosition = new Vector3(-500, transform.localPosition.y, transform.localPosition.z);
            var moveSpeed = 2500 * Time.deltaTime;
            if (isBackward)
            {
                foreach (var button in _buttons) button.enabled = false;
                while (sprite.alpha > 0)
                {
                    sprite.alpha = Mathf.MoveTowards(sprite.alpha, 0, 4 * Time.deltaTime);
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, hiddenPosition, moveSpeed);
                    yield return null;
                }
            }
            else
            {
                while (sprite.alpha < 1)
                {
                    sprite.alpha = Mathf.MoveTowards(sprite.alpha, 1, 4 * Time.deltaTime);
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, visiblePosition, moveSpeed);
                    yield return null;
                }
                foreach (var button in _buttons) button.enabled = true;
            }
        }
    }
}