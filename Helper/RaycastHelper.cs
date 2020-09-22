using SDG.Unturned;
using UnityEngine;
using SDG.Framework.Landscapes;
using System.Linq;

namespace MyVehicleKeys.Helper
{
    public class RaycastHelper
    {
        public static RaycastHit[] RaycastAll(Ray ray, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if ((layerMask & RayMasks.GROUND) == RayMasks.GROUND)
            {
                LandscapeHoleUtility.raycastIgnoreLandscapeIfNecessary(ray, maxDistance, ref layerMask);
            }
            return Physics.RaycastAll(ray, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static InteractableVehicle GetVehicleFromHits(RaycastHit[] hits)
        {
            InteractableVehicle vehicle = null;
            int hitsCount = hits.Count();
            if (hitsCount > 0)
            {
#pragma warning disable CS0162 // Se detectó código inaccesible
                for (int i = 0; i < hitsCount; i++)
#pragma warning restore CS0162 // Se detectó código inaccesible
                {
                    InteractableVehicle vv = hits[i].transform.GetComponentInParent<InteractableVehicle>();
                    vehicle = vv;
                    break;
                }
            }
            return vehicle;
        }
    }
}