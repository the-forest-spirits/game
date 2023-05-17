using TMPro;
using UnityEngine;
using UnityEngine.Events;

/**
 * Represents a GameObject that can have dialogue.
 * References conversations and branches written in the editor.
 */
public class Talker : MonoBehaviour, IClickable
{
    #region Unity fields

    [Tooltip("Where the text will show up")]
    public TextMeshPro textRef;

    [Tooltip("Where new conversations will start")]
    public Branch startBranch;

    [Header("Events")]
    public UnityEvent onStart;

    public UnityEvent onEnd;
    public UnityEvent onNext;
    private bool fading;

    #endregion

    #region Private variables

    [SerializeField, ReadOnly]
    private int _index = 0;

    [SerializeField, ReadOnly]
    private bool _talking = false;

    [SerializeField, ReadOnly]
    private Conversation _currentConversation;

    #endregion

    #region Public methods

    /** Call this method to start or progress a conversation */
    public void OnInteract() {
        if (!_talking) {
            StartConversation();
        }
        else {
            //Only run this if the conversation is not CURRENTLY fading out!
            if (!fading) {
                if (_index >= 0) {
                    //if there's an onEnd method, invoke it now -- this will usually be the call to fadeout
                    _currentConversation.dialogue[_index].onEnd.Invoke();
                }

                fading = true;
                //wait for the amount of time specified in waitTime, then call next -- waittime in this case
                //is the time you need for the text to fade OUT
                this.WaitThen(_currentConversation.dialogue[_index].waitTime, () => {
                    Next();
                    fading = false;
                });
            }
            //Next();
        }
    }

    /** Starts a conversation from scratch */
    public void StartConversation() {
        if (_talking) Teardown();

        _currentConversation = startBranch.GetConversation();
        if (_currentConversation == null) return;
        _currentConversation.onStart.Invoke();

        _talking = true;
        _index = -1;
        Setup();
        Next();
    }

    #endregion

    #region Private helper functions

    private void Setup() {
        onStart.Invoke();
    }

    private void Teardown() {
        onEnd.Invoke();
        _talking = false;
        _index = 0;
        _currentConversation = null;
        textRef.text = "";
    }

    private void Next() {
        _index++;

        // If there's still text to go...
        if ((_index < _currentConversation.dialogue.Length)) {
            onNext.Invoke();

            _currentConversation.dialogue[_index].onStart.Invoke();

            textRef.text = _currentConversation.dialogue[_index].text;
            return;
        }

        // Otherwise, try to move to the next Branch (or finish)
        _currentConversation.onEnd.Invoke();

        // If there's nowhere to go, stop.
        if (_currentConversation.andThen == null) {
            Teardown();
            return;
        }

        // If the next branch leads nowhere, stop.
        _currentConversation = _currentConversation.andThen.GetConversation();
        if (_currentConversation == null) {
            Teardown();
            return;
        }

        // Otherwise continue to the beginning of the next conversation.
        _index = -1;
        _currentConversation.onStart.Invoke();
        Next();
    }

    #endregion

    public bool OnClick(Vector2 screenPos, Camera cam) {
        if (!_talking || _currentConversation == null) {
            return false;
        }

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textRef, screenPos, cam);
        if (linkIndex == -1) {
            return false;
        }

        var info = textRef.textInfo.linkInfo[linkIndex];
        string linkId = info.GetLinkID();

        _currentConversation.dialogue[_index].TriggerLink(linkId);

        return true;
    }

    public bool IsClickable(Vector2 screenPos, Camera cam, out Sprite customSprite) {
        customSprite = null;
        if (!_talking || _currentConversation == null) {
            return false;
        }

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textRef, screenPos, cam);
        if (linkIndex == -1) {
            return false;
        }

        return true;
    }
}