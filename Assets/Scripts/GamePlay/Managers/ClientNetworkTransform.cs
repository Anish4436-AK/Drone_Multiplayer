using Unity. Netcode.Components;
using UnityEngine;

namespace Unity.Multiplayer.Samples.Utilities.ClientAuthority
{ 

/// <summary>
/// Used for syncing a transform with client side changes. This in
/// for transforms that'll always be owned by the server.
/// </summary>
[DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {

/// <summary>
/// Used to determine who can write to this transform. Owner c.
/// This imposes state to the server. This is putting trust on
/// </summary>
            protected override bool OnIsServerAuthoritative()
            {

                    return false;

            }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Debug.Log(
                $"CNT Spawn | Object:{NetworkObjectId} " +
                $"Owner:{OwnerClientId} " +
                $"IsOwner:{IsOwner}");
        }
    }

}

