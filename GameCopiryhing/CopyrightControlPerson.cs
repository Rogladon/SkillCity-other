using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SkillCity.GameProduktolog;
//Это  контроллер персонажа в одной из многих мини-игр
//встроенных в SkillCity
namespace SkillCity.GameCopyright {
	public class CopyrightControlPerson : MonoBehaviour {
		public float speed;
		public float jumpForce;
		public Transform cam;
		private Rigidbody2D rigid;
		public float distCam;
		public float speedCam;
		private bool startRun;
		private Vector3 startCamPos;
		private Vector3 deltaCam;
		public int baseJumpsCount = 2;
		public int maxJumps {
			get {
				if (canBonusJump) {
					return baseJumpsCount + 1;
				} else {
					return baseJumpsCount;
				}
			}
		}
		public int jumpsCount = 0;
		public bool canJump { get { return jumpsCount < maxJumps; } }

		public GameObject[] sprJumps;

		public float distanceRay;
		public Transform footPoint;
		public int MaxScore;
		public int Score;

		public bool canBonusJump = false;

		public float maxGlideCharge = 5;
		[Tooltip("Восстановление за секунду")]
		public float glideChargeRestorationSpeed = 1f;
		[HideInInspector]
		public float glideCharge;
		public float glideChargePerBonus = 3f;

		public bool TypeCam;
		bool jump = true;
		public Text scoreText;
		public GameObject win;
		public GameObject died;
		public GameObject pause;
		public GameObject hint;
		public Transform FullBacground;
		public GameObject CloubFirstPref;
		public float speedCloubFirst;
		public GameObject CloudSecondPref;
		public float speedCloudSecond;
		public GameObject CityFirstPref;
		public float speedCityFirst;
		public GameObject CitySecondPref;
		public float speedCitySecond;

		public DragonBones.UnityArmatureComponent anim;
		public GameObject blur;
		public Material BLUR;
		public Material standart;

		public TrailRenderer trail;

		public GameObject txtTapToStart;
		public Slider glideChargeBar;

		public PaperPlaneController paperPlanePrefab;
		public Transform paperPlaneSpawnPoint;

		enum Status {
			jump,
			doublejump,
			fall,
			run,
			idle,
			fall_down,
			glide_intro,
			glide,
		}
		Status status;

		public float downSpeed;
		public GameObject gameMenuPrefab;
		private GameMenu gameMenu;

		public Button btnJump;
		public Sprite firstJump;
		public Sprite secondJump;
		public Image jumpButton;
		public Button btnGlide;
		public ScoredController scored;

		float originalGravityScale;
		public float gravityScaleOnGliding = 0.5f;


		// Start is called before the first frame update
		void Start() {
			GameState.audioController?.LoopPlay(GameState.AudioClips.platformTheme);
			status = Status.idle;
			gameMenu = Instantiate(gameMenuPrefab).GetComponent<GameMenu>();
			gameMenu.ShowHint();
			GameState.status = GameState.Status.Null;
			Screen.orientation = ScreenOrientation.Landscape;
			rigid = GetComponent<Rigidbody2D>();
			startCamPos = cam.position;
			deltaCam = transform.position - cam.position;
			glideCharge = maxGlideCharge;

			glideChargeBar.maxValue = maxGlideCharge;
			originalGravityScale = rigid.gravityScale;

			if (paperPlaneSpawnPoint == null) {
				Debug.LogError("paperPlaneSpawnPoint is not set");
			}
		}

		// Update is called once per frame

		private void FixedUpdate() {
			scoreText.text = Score.ToString() + "/" + MaxScore.ToString();
			if (transform.position.y < -10) {
				gameMenu.Died();
			}
			if (Score == MaxScore) {
				Win();
			}
			if (TypeCam) {
				Vector3 posCam = transform.position;
				posCam.y = startCamPos.y;
				cam.position = posCam - deltaCam;

			} else {
				Vector3 posCam = transform.position;
				if (posCam.y < -5) {
					posCam.y = -5;
				}
				cam.position = posCam - deltaCam + Vector3.back;
				posCam = posCam - deltaCam + Vector3.back;
				posCam.z = 0;
				FullBacground.position = posCam;
			}
			if (startRun && status != Status.fall_down) {
				transform.position += Vector3.right * speed * Time.fixedDeltaTime;
			}

			if (isGliding) {
				if (rigid.gravityScale == originalGravityScale) {
					rigid.velocity = new Vector2(rigid.velocity.x, 0);
				}
				rigid.gravityScale = gravityScaleOnGliding;
			} else {
				rigid.gravityScale = originalGravityScale;
			}
		}

		void UpdateAnim(Status st) {
			if (status != st) {
				switch (st) {
				case Status.idle:
					anim.animation.FadeIn("idle");
					break;
				case Status.run:
					anim.animation.FadeIn("run");
					break;
				case Status.jump:
					anim.animation.FadeIn("jump");
					break;
				case Status.doublejump:
					anim.animation.FadeIn("doublejump", -1, 1);
					break;
				case Status.fall:
					anim.animation.FadeIn("fall");
					break;
				case Status.fall_down:
					anim.animation.FadeIn("fall_down", -1, 1);
					break;
				case Status.glide_intro:
					anim.animation.FadeIn("glide_intro", -1, 1);
					break;
				case Status.glide:
					anim.animation.FadeIn("glide");
					break;
				}
			}
		}

		float tapToStartDeltaTime = 0f;
		bool doubleJumpAnimHasBeenCompleted = false;
		void Update() {
			if(jumpsCount == 0) {
				jumpButton.sprite = firstJump;
			}
			else {
				if(jumpsCount == 1) {
					jumpButton.sprite = secondJump;
				}
			}

			for (int i = 0; i < sprJumps.Length; i++) {
				sprJumps[i].SetActive(i < (maxJumps - jumpsCount));
			}
			if (canBonusJump && (maxJumps - jumpsCount) > 1) {
				// TODO: менять цвет, когда остался последний бонусный прыжок
			}

			btnGlide.interactable = canGlide;

			{
				tapToStartDeltaTime += Time.deltaTime;
				var alpha = (System.Math.Cos(tapToStartDeltaTime / 0.5) + 1.0) / 2.0;
				Text txt = txtTapToStart.GetComponent<Text>();
				txt.color = txt.GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(txt.color.r, txt.color.g, txt.color.b, (float)alpha);
			}

			if (glideCharge > maxGlideCharge) {
				glideCharge = maxGlideCharge;
				canBonusJump = true;
			}

			if (isGliding && canGlide) {
				glideCharge -= Time.deltaTime;
			}

			//trail.enabled = justStopped;

			RaycastHit2D hit;
			if (startRun) {
				if (hit = Physics2D.Raycast(footPoint.position, Vector2.down, distanceRay)) {
					if (!isGliding && !hit.transform.CompareTag("Player") && jump) {
						if (status == Status.fall_down) {
							if (anim.animation.isCompleted) {
								UpdateAnim(Status.run);
								status = Status.run;
							}
						} else {
							UpdateAnim(Status.run);
							status = Status.run;
						}
						jumpsCount = 0;
					}

				} else if (status == Status.fall_down) {
					if (anim.animation.isCompleted) {
						UpdateAnim(Status.run);
						status = Status.run;
					}
				} else if (status == Status.glide_intro) {
					if (anim.animation.isCompleted) {
						UpdateAnim(Status.glide);
						status = Status.glide;
					}
				}
				if (!isGliding) {
					if (rigid.velocity.y > 0) {
						if (doubleJumpAnimHasBeenCompleted) UpdateAnim(Status.jump);
						status = Status.jump;
					} else if (rigid.velocity.y < 0) {
						UpdateAnim(Status.fall);
						status = Status.fall;
					}
				}
				jump = true;
			}

			if (status != Status.idle) {
				txtTapToStart.SetActive(false);
			}

			glideChargeBar.value = glideCharge;
			if (isGliding && !canGlide) {
				StopGlide();
			}

			trail.enabled = status == Status.glide && status == Status.glide_intro;
			btnJump.interactable = jumpsCount < maxJumps;

			if (status == Status.jump) {
				if (anim.animation.isCompleted) {
					doubleJumpAnimHasBeenCompleted = true;
				}
			}
		}

		public bool isOnGround {
			get {
				RaycastHit2D hit;
				if (hit = Physics2D.Raycast(footPoint.position, Vector2.down, distanceRay)) {
					return !hit.transform.CompareTag("Player");
				}
				return false;
			}
		}

		public bool canGlide {
			get {
				return glideCharge > 0 && status != Status.idle && !isOnGround;
			}
		}
		public bool isGliding { get; private set; } = false;

		bool glided = false;
		public void StartGlide() {
			if (canGlide) {
				isGliding = true;
				UpdateAnim(Status.glide_intro);
				status = Status.glide_intro;
				glided = true;
			}
		}

		public void StopGlide() {
			isGliding = false;
			if (glided) {
				Instantiate(paperPlanePrefab, paperPlaneSpawnPoint.transform.position, transform.rotation);
				glided = false;
			}
		}

		public void DoJump() {
			if (jumpsCount >= maxJumps || isGliding) {
				return;
			}
			jump = false;
			jumpsCount++;
			if (jumpsCount > baseJumpsCount) {
				canBonusJump = false;
			}
			rigid.velocity = Vector2.zero;
			rigid.velocity = Vector2.zero;
			rigid.AddForce(Vector3.up * jumpForce, ForceMode2D.Force);
			startRun = true;
			if (jumpsCount > 1) {
				UpdateAnim(Status.doublejump);
				doubleJumpAnimHasBeenCompleted = false;
			} else {
				UpdateAnim(Status.jump);
			}
		}


		public void OffBlur() {
			blur.GetComponent<Image>().material = standart;
			blur.SetActive(false);
		}
		void Win() {
			rigid.bodyType = RigidbodyType2D.Static;
			Invoke("GoWin", 0.2f);
		}

		private void OnTriggerEnter2D(Collider2D collision) {
			if (collision.transform.tag == "Coin") {
				scored.AddItem(collision.transform.GetComponentInChildren<SpriteRenderer>().sprite);
				Score++;
				Destroy(collision.gameObject);
				GameState.audioController?.SinglePlay(GameState.AudioClips.collect);
			} else if (collision.transform.CompareTag("GameCopyright Bonus Charge")) {
				Debug.Log("+Charge");
				glideCharge += glideChargePerBonus;
				Destroy(collision.gameObject);
				GameState.audioController?.SinglePlay(GameState.AudioClips.collect);
			}
		}
		public void GoWin() {
			GameState.GameWin((int)(100 * transform.position.x));
		}
	}
}
