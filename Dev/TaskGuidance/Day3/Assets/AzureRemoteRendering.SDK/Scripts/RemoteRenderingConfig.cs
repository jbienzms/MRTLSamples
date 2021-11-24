// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using UnityEngine;

namespace Microsoft.Azure.RemoteRendering.Unity
{
    /// <summary>
    /// This menu item generates an optional configuration file which can be
    /// excluded from source control to avoid committing credentials there.
    /// </summary>
    [CreateAssetMenu(fileName = "RemoteRenderingConfig", menuName = "Azure Remote Rendering/Configuration")]
    public class RemoteRenderingConfig : ScriptableObject
    {
        #region Unity Inspector Variables
        [Header("Authentication")]
        [SerializeField]
        [Tooltip("The domain to use when authenticating with the Remote Rendering service.")]
        private string accountDomain = "";
        public string AccountDomain => accountDomain;

        [SerializeField]
        [Tooltip("The Account ID provided by the Remtoe Rendering service portal.")]
        private string accountId = "";
        public string AccountId => accountId;

        [SerializeField]
        [Tooltip("The Account Key provided by the Remote Rendering service portal.")]
        protected string accountKey = "";
        public string AccountKey => accountKey;


        [Header("Connection")]
        [SerializeField]
        [Tooltip("The domain to use when connecting to the Remote Rendering service.")]
        private string remoteRenderingDomain = "";
        public string RemoteRenderingDomain => remoteRenderingDomain;
        #endregion // Unity Inspector Variables
    }
}
