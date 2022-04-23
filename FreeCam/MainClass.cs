﻿using System;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;

namespace FreeCam
{
    class MainClass : ModBehaviour
    {
        public static GameObject _freeCam;
        public static Camera _camera;
        public static OWCamera _OWCamera;

        public static float _moveSpeed = 0.1f;
        public static bool inputEnabled = false;

        InputMode _storedMode;
        bool mode = false;
        public bool _disableLauncher;
        public int _fov;

        private GameObject _probeLauncher;

        private bool _isInitialized;

        public void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public override void Configure(IModConfig config)
        {
            _disableLauncher = config.GetSettingsValue<bool>("disableLauncher");
            _fov = config.GetSettingsValue<int>("fov");

            // If the mod is currently active we can set these immediately
            if (_camera != null)
            {
                _camera.fieldOfView = _fov;
                _OWCamera.fieldOfView = _fov;

                // Only update launcher if we are currently using the freecam
                if (Locator.GetActiveCamera() == _OWCamera) SetLauncher(!_disableLauncher);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode _)
        {
            base.ModHelper.Console.WriteLine($"[FreeCam] : Loading scene {scene.name}");

            if (scene.name != "SolarSystem" && scene.name != "EyeOfTheUniverse") return;

            PreInit();
        }

        private void PreInit()
        {
            base.ModHelper.Console.WriteLine("[FreeCam] : Pre-Initializing");

            try
            {
                _freeCam = new GameObject();
                _freeCam.SetActive(false);
                _camera = _freeCam.AddComponent<Camera>();
                _camera.clearFlags = CameraClearFlags.Color;
                _camera.backgroundColor = Color.black;
                _camera.fieldOfView = _fov;
                _camera.nearClipPlane = 0.1f;
                _camera.farClipPlane = 40000f;
                _camera.depth = 0f;
                _camera.enabled = false;

                _freeCam.AddComponent<CustomLookAround>();
                _freeCam.AddComponent<CustomFlashlight>();

                _OWCamera = _freeCam.AddComponent<OWCamera>();
                _OWCamera.renderSkybox = true;

                _freeCam.SetActive(true);

                _isInitialized = false;
            }
            catch (Exception ex)
            {
                base.ModHelper.Console.WriteLine($"[FreeCam] : Failed pre-initialization: {ex.Message}, {ex.StackTrace}");
            }
        }

        private void Init()
        {
            base.ModHelper.Console.WriteLine("[FreeCam] : Initializing");

            try
            {
                if (_disableLauncher) SetLauncher(false);

                if (_isInitialized)
                {
                    base.ModHelper.Console.WriteLine("[FreeCam] : Already set up! Aborting...");
                }
                else
                {
                    _freeCam.transform.parent = Locator.GetPlayerTransform();

                    _freeCam.transform.position = Locator.GetPlayerTransform().position;
                    _freeCam.SetActive(false);

                    FlashbackScreenGrabImageEffect temp = _freeCam.AddComponent<FlashbackScreenGrabImageEffect>();
                    temp._downsampleShader = Locator.GetPlayerCamera().gameObject.GetComponent<FlashbackScreenGrabImageEffect>()._downsampleShader;

                    PlanetaryFogImageEffect _image = _freeCam.AddComponent<PlanetaryFogImageEffect>();
                    _image.fogShader = Locator.GetPlayerCamera().gameObject.GetComponent<PlanetaryFogImageEffect>().fogShader;

                    PostProcessingBehaviour _postProcessiong = _freeCam.AddComponent<PostProcessingBehaviour>();
                    _postProcessiong.profile = Locator.GetPlayerCamera().gameObject.GetAddComponent<PostProcessingBehaviour>().profile;

                    _freeCam.SetActive(true);
                    _camera.cullingMask = Locator.GetPlayerCamera().mainCamera.cullingMask & ~(1 << 27) | (1 << 22);

                    _freeCam.name = "FREECAM";

                    _isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                base.ModHelper.Console.WriteLine($"[FreeCam] : Failed initialization: {ex.Message}, {ex.StackTrace}");
            }
        }

        void SetLauncher(bool enable)
        {
            // Fine to search for it when this is first called bc by default it will be active
            if (_probeLauncher == null) _probeLauncher = GameObject.Find("Player_Body/PlayerCamera/ProbeLauncher");
            _probeLauncher.SetActive(enable);

            base.ModHelper.Console.WriteLine($"[FreeCam] : Launcher {(enable ? "on" : "off")}!");
        }

        void Update()
        {
            if (inputEnabled)
            {
                if (Keyboard.current[Key.UpArrow].wasPressedThisFrame)
                {
                    Init();
                }

                if (Keyboard.current[Key.DownArrow].wasPressedThisFrame)
                {
                    _moveSpeed = 0.1f;
                }

                if (Keyboard.current[Key.LeftArrow].wasPressedThisFrame)
                {
                    if (Locator.GetPlayerSuit().IsWearingHelmet())
                    {
                        Locator.GetPlayerSuit().RemoveHelmet();
                    }
                    else
                    {
                        Locator.GetPlayerSuit().PutOnHelmet();
                    }
                }

                if (Keyboard.current[Key.NumpadDivide].wasPressedThisFrame || Keyboard.current[Key.Comma].wasPressedThisFrame)
                {
                    Time.timeScale = 0f;
                }

                if (Keyboard.current[Key.NumpadMultiply].wasPressedThisFrame || Keyboard.current[Key.Period].wasPressedThisFrame)
                {
                    Time.timeScale = 0.5f;
                }

                if (Keyboard.current[Key.NumpadMinus].wasPressedThisFrame || Keyboard.current[Key.Slash].wasPressedThisFrame)
                {
                    Time.timeScale = 1f;
                }

                if (Keyboard.current[Key.Numpad0].wasPressedThisFrame || Keyboard.current[Key.Digit0].wasPressedThisFrame)
                {
                    _freeCam.transform.parent = Locator.GetPlayerTransform();
                    _freeCam.transform.position = Locator.GetPlayerTransform().position;
                }

                if (Keyboard.current[Key.Numpad1].wasPressedThisFrame || Keyboard.current[Key.Digit1].wasPressedThisFrame)
                {
                    var go = Locator.GetAstroObject(AstroObject.Name.Sun).gameObject.transform;
                    _freeCam.transform.parent = go;
                    _freeCam.transform.position = go.position;
                }

                if (Keyboard.current[Key.Numpad2].wasPressedThisFrame || Keyboard.current[Key.Digit2].wasPressedThisFrame)
                {
                    var go = Locator.GetAstroObject(AstroObject.Name.Comet).gameObject.transform;
                    _freeCam.transform.parent = go;
                    _freeCam.transform.position = go.position;
                }

                if (Keyboard.current[Key.Numpad3].wasPressedThisFrame || Keyboard.current[Key.Digit3].wasPressedThisFrame)
                {
                    var go = Locator.GetAstroObject(AstroObject.Name.CaveTwin).gameObject.transform;
                    _freeCam.transform.parent = go;
                    _freeCam.transform.position = go.position;
                }

                if (Keyboard.current[Key.Numpad4].wasPressedThisFrame || Keyboard.current[Key.Digit4].wasPressedThisFrame)
                {
                    var go = Locator.GetAstroObject(AstroObject.Name.TowerTwin).gameObject.transform;
                    _freeCam.transform.parent = go;
                    _freeCam.transform.position = go.position;
                }

                if (Keyboard.current[Key.Numpad5].wasPressedThisFrame || Keyboard.current[Key.Digit5].wasPressedThisFrame)
                {
                    var go = Locator.GetAstroObject(AstroObject.Name.TimberHearth).gameObject.transform;
                    _freeCam.transform.parent = go;
                    _freeCam.transform.position = go.position;
                }

                if (Keyboard.current[Key.Numpad6].wasPressedThisFrame || Keyboard.current[Key.Digit6].wasPressedThisFrame)
                {
                    var go = Locator.GetAstroObject(AstroObject.Name.BrittleHollow).gameObject.transform;
                    _freeCam.transform.parent = go;
                    _freeCam.transform.position = go.position;
                }

                if (Keyboard.current[Key.Numpad7].wasPressedThisFrame || Keyboard.current[Key.Digit7].wasPressedThisFrame)
                {
                    var go = Locator.GetAstroObject(AstroObject.Name.GiantsDeep).gameObject.transform;
                    _freeCam.transform.parent = go;
                    _freeCam.transform.position = go.position;
                }

                if (Keyboard.current[Key.Numpad8].wasPressedThisFrame || Keyboard.current[Key.Digit8].wasPressedThisFrame)
                {
                    var go = Locator.GetAstroObject(AstroObject.Name.DarkBramble).gameObject.transform;
                    _freeCam.transform.parent = go;
                    _freeCam.transform.position = go.position;
                }

                if (Keyboard.current[Key.Numpad9].wasPressedThisFrame || Keyboard.current[Key.Digit9].wasPressedThisFrame)
                {
                    var go = Locator.GetAstroObject(AstroObject.Name.RingWorld).gameObject.transform;
                    _freeCam.transform.parent = go;
                    _freeCam.transform.position = go.position;
                }

                var scrollInOut = Mouse.current.scroll.y.ReadValue();
                _moveSpeed += scrollInOut * 0.05f;
                if (_moveSpeed < 0)
                {
                    _moveSpeed = 0;
                }

                if (Keyboard.current[Key.NumpadPeriod].wasPressedThisFrame || Keyboard.current[Key.Semicolon].wasPressedThisFrame)
                {
                    if (mode)
                    {
                        // Switch back to regular camera

                        mode = false;
                        if (_storedMode == InputMode.None)
                        {
                            _storedMode = InputMode.Character;
                        }

                        OWInput.ChangeInputMode(_storedMode);
                        GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", Locator.GetPlayerCamera());
                        _camera.enabled = false;
                        Locator.GetActiveCamera().mainCamera.enabled = true;

                        // Turn the launcher back on
                        SetLauncher(true);
                    }
                    else
                    {
                        // Switch to freecam

                        if (!_isInitialized) Init();

                        mode = true;
                        _storedMode = OWInput.GetInputMode();
                        OWInput.ChangeInputMode(InputMode.None);
                        GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _OWCamera);
                        Locator.GetActiveCamera().mainCamera.enabled = false;
                        _camera.enabled = true;

                        SetLauncher(!_disableLauncher);
                    }
                }
            }
        }

        public static void MNActivateInput()
        {
            inputEnabled = true;
        }

        public static void MNDeactivateInput()
        {
            inputEnabled = false;
        }
    }
}