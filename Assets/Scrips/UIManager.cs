using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;

namespace Scrips
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private NetworkManager networkManager;

        [SerializeField]
        private Button startHostButton;

        [SerializeField]
        private Button startClientButton;

        private void Awake()
        {
            Cursor.visible = true;
        }

        private void OnEnable()
        {
            startClientButton.onClick.AddListener(StartClientButtonOnClicked);
            startHostButton.onClick.AddListener(StartHostButtonOnClicked);
        }

        private void OnDisable()
        {
            startClientButton.onClick.RemoveListener(StartClientButtonOnClicked);
            startHostButton.onClick.RemoveListener(StartHostButtonOnClicked);
        }

        private void StartHostButtonOnClicked()
        {
            networkManager.StartHost();
        }

        private void StartClientButtonOnClicked()
        {
            networkManager.StartClient();
        }

        void Start()
        {
        
        }

        void Update()
        {
        
        }
    }
}
