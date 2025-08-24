using System;
using System.Collections;
using UnityEngine;

namespace DialogueSystem
{
    public class DialogueHolder : MonoBehaviour
    {
        private PlayerController playerController;
        private IEnumerator dialogueSeq;
        private bool dialogueFinished;

        private void Start()
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        private void OnEnable()
        {
            dialogueSeq = DialogueSequence();
            StartCoroutine(dialogueSeq);
        }

        private void Update()
        {
            if (playerController.isClosingDialogue)
            {
                Deactivate();
                gameObject.SetActive(false);
                StopCoroutine(dialogueSeq);
            }
        }

        private IEnumerator DialogueSequence()
        {
            if (!dialogueFinished)
            {
                for (int i = 0; i < transform.childCount - 1; i++)
                {
                    Deactivate();
                    transform.GetChild(i).gameObject.SetActive(true);
                    yield return new WaitUntil(() => transform.GetChild(i).GetComponent<DialogueLine>().finished);
                    yield return new WaitForSeconds(0.25f);
                }
            }
            else
            {
                int index = transform.childCount - 1;
                Deactivate();
                transform.GetChild(index).gameObject.SetActive(true);
                yield return new WaitUntil(() => transform.GetChild(index).GetComponent<DialogueLine>().finished);
                yield return new WaitForSeconds(0.25f);
            }

            dialogueFinished = true;
            gameObject.SetActive(false);
        }

        private void Deactivate()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
