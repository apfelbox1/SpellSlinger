namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using Spellslinger.Game.Environment;
    using Spellslinger.Game.Manager;
    using UnityEngine;

    public class Level01Manager : MonoBehaviour {
        [Header("First Puzzle | Fence")]
        [SerializeField] private Torch fenceTorch;
        [SerializeField] private Animator fenceAnimator;
        [SerializeField] private AudioClip fenceOpenSound;

        [Header("Second Puzzle | Mirrors")]
        [SerializeField] private Torch[] mirrorTorches;
        [SerializeField] private Torch wrongMirrorTorch;
        [SerializeField] private Transform playerHead;
        [SerializeField] private Transform playerOrigin;
        private bool isMirrorPuzzleSolved = false;
        private int walkedInCircles = 0;

        // const with offset to mirror room
        private const float roomOffsetX = 29.5f;
        private const float roomOffsetY = 0.0f;
        private const float roomOffsetZ = -30f;

        [Header("Sounds")]
        [SerializeField] private AudioClip puzzleSolvedSound;
        [SerializeField] private AudioClip puzzleFailSound;


        [Header("Wizard Voice")]
        [SerializeField] private AudioSource wizardVoiceAudioSource;
        [SerializeField] private AudioClip thatsNotRight;
        [SerializeField] private AudioClip mirrorPuzzleHint01;
        [SerializeField] private AudioClip mirrorPuzzleHint02;
        [SerializeField] private AudioClip mirrorPuzzleHint03;
        [SerializeField] private AudioClip mirrorPuzzleHint04;
        [SerializeField] private AudioClip mirrorPuzzleSolved;
        [SerializeField] private AudioClip walkedInCircles1;
        [SerializeField] private AudioClip walkedInCircles2;
        [SerializeField] private AudioClip walkedInCircles3;
        [SerializeField] private AudioClip walkedInCircles100;
        [SerializeField] private AudioClip paintingOldPetunia;
        [SerializeField] private AudioClip paintingHaplessPercival;
        [SerializeField] private AudioClip paintingLucius;
        [SerializeField] private AudioClip paintingCedric;
        private float wizardVoiceHintTimer = 15.0f;

        private bool paintingOldPetuniaTriggered = false;
        private bool paintingHaplessPercivalTriggered = false;
        private bool paintingLuciusTriggered = false;
        private bool paintingCedricTriggered = false;

        private void Start() {
            this.fenceTorch.OnTorchLit = () => {
                this.OnFenceTorchLit();
            };

            this.wrongMirrorTorch.OnTorchLit = () => {
                this.OnWrongMirrorTorchLit();
            };

            foreach (Torch torch in this.mirrorTorches) {
                torch.OnTorchLit = () => {
                    this.OnMirrorTorchLit();
                };
            }
        }

        private void Update() {
            if (this.isMirrorPuzzleSolved) {
                return;
            }


        }

        /// <summary>
        /// Coroutine that plays an audioclip for the wizard after a delay.
        /// </summary>
        /// <param name="clip">The audioclip to play.</param>
        private IEnumerator PlayWizardVoiceDelayed(AudioClip clip, float delay) {
            yield return new WaitForSeconds(delay);
            this.PlayWizardVoice(clip);
        }

        /// <summary>
        /// Plays an audioclip for the wizard.
        /// </summary>
        /// <param name="clip">The audioclip to play.</param>
        private void PlayWizardVoice(AudioClip clip) {
            this.wizardVoiceAudioSource.Stop();
            this.wizardVoiceAudioSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Called when the fence torch is lit. Lowers the fence.
        /// </summary>
        private void OnFenceTorchLit() {
            this.fenceAnimator.SetBool("isFenceDown", true);
            this.fenceAnimator.gameObject.GetComponent<AudioSource>().PlayOneShot(this.fenceOpenSound);
        }

        /// <summary>
        /// Called when a mirror torch is lit. Checks if all torches are lit.
        /// </summary>
        private void OnMirrorTorchLit() {
            if (this.isMirrorPuzzleSolved) {
                return;
            }

            // check if all torches are lit
            foreach (Torch torch in this.mirrorTorches) {
                if (!torch.IsLit) {
                    return;
                }
            }

            // puzzle solved
            this.isMirrorPuzzleSolved = true;
            this.GetComponent<BoxCollider>().enabled = false;
            GameManager.Instance.PlayAudioClip(this.puzzleSolvedSound);
            this.StartCoroutine(this.PlayWizardVoiceDelayed(this.mirrorPuzzleSolved, .5f));
        }

        /// <summary>
        /// Called when the wrong mirror torch is lit. Extinguishes all torches.
        /// </summary>
        private IEnumerator OnWrongMirrorTorchLit() {
            GameManager.Instance.PlayAudioClip(this.puzzleFailSound);
            this.wrongMirrorTorch.ExtinguishTorch();

            // extinguish all torches
            foreach (Torch torch in this.mirrorTorches) {
                torch.ExtinguishTorch();
            }

            yield return new WaitForSeconds(0.5f);

            this.PlayWizardVoice(this.thatsNotRight);
        }

        /// <summary>
        /// Sets the player position to first room that looks exactly the same without the player knowing.
        /// </summary>
        private void ResetPlayerPosition() {
            this.walkedInCircles++;

            this.playerHead.position = new Vector3(this.playerHead.position.x - roomOffsetX, this.playerHead.position.y - roomOffsetY, this.playerHead.position.z - roomOffsetZ);
            this.playerOrigin.position = new Vector3(this.playerOrigin.position.x - roomOffsetX, this.playerOrigin.position.y - roomOffsetY, this.playerOrigin.position.z - roomOffsetZ);

            if (this.walkedInCircles == 1) {
                this.PlayWizardVoice(this.walkedInCircles1);
            } else if (this.walkedInCircles == 2) {
                this.PlayWizardVoice(this.walkedInCircles2);
            } else if (this.walkedInCircles == 3) {
                this.PlayWizardVoice(this.walkedInCircles3);
            } else if (this.walkedInCircles == 100) {
                this.PlayWizardVoice(this.walkedInCircles100);
            }
        }

        /// <summary>
        /// Triggers the wizard to say something about a painting.
        /// </summary>
        public void TriggerPaintingSpeech(string paintingName, Transform paintingPosition) {
            // check if the player is looking in the direction of the painting
            Vector3 playerDirection = this.playerHead.forward;
            Vector3 playerToPainting = paintingPosition.position - this.playerHead.position;
            float angle = Vector3.Angle(playerDirection, playerToPainting);

            if (angle > 45f) {
                return;
            }

            if (paintingName == "oldPetunia" && !this.paintingOldPetuniaTriggered) {
                this.paintingOldPetuniaTriggered = true;
                this.PlayWizardVoice(this.paintingOldPetunia);
            } else if (paintingName == "haplessPercival" && !this.paintingHaplessPercivalTriggered) {
                this.paintingHaplessPercivalTriggered = true;
                this.PlayWizardVoice(this.paintingHaplessPercival);
            } else if (paintingName == "lucius" && !this.paintingLuciusTriggered) {
                this.paintingLuciusTriggered = true;
                this.PlayWizardVoice(this.paintingLucius);
            } else if (paintingName == "cedric" && !this.paintingCedricTriggered) {
                this.paintingCedricTriggered = true;
                this.PlayWizardVoice(this.paintingCedric);
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Player") && !this.isMirrorPuzzleSolved) {
                this.ResetPlayerPosition();
            }
        }
    }
}