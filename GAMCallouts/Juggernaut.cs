using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;

namespace GAMCallouts
{
    [CalloutProperties(name: "Juggernaut Terrorist", author: "GAM Studios", version: "0.0.1")]
    public class Juggernaut : Callout
    {
        Ped suspect;

        private Vector3[] coordinates =
        {
            new Vector3(-1083f, -3385f, 14f),
            new Vector3(-1828f, -2823f, 14f),
            new Vector3(-1265f, -2652f, 14f),
            new Vector3(-932f, -2559f, 14f),
            new Vector3(-1021f, -2743f, 14f),
            new Vector3(-1262f, -3411f, 14f)
        };

        public Juggernaut()
        {
            InitInfo(coordinates[RandomUtils.Random.Next(coordinates.Length)]);
            ShortName = "Juggernaut Terrorist";
            CalloutDescription = "A Juggernaut has taken the airport hostage. Respond Code 3.";
            ResponseCode = 3;
            StartDistance = 65f;
        }

        public override async Task OnAccept()
        {
            var suspects = (PedHash)2431602996;
            var gun = WeaponHash.Minigun;
            InitBlip();
            suspect = await SpawnPed(suspects, Location);
            suspect.MaxHealthFloat = 9000f;
            suspect.HealthFloat = 9000f;
            suspect.AttachBlip();
            suspect.AlwaysKeepTask = true;
            suspect.BlockPermanentEvents = true;
            suspect.ArmorFloat = 3000f;
            suspect.CanRagdoll = false;
            suspect.IsMeleeProof = true;
            suspect.CanSufferCriticalHits = false;
            suspect.Weapons.Give(gun, 9999, true, true);
            UpdateData();
        }

        public override void OnStart(Ped player)
        {
            base.OnStart(player);
            suspect.Accuracy = 99;
            suspect.FiringPattern = FiringPattern.FullAuto;
            suspect.ShootRate = 1000;
            suspect.Task.FightAgainst(player);
            ShowNetworkedNotification("The suspect should be considered armed and dangerous with military weaponry.",
                "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "Notification", 35f);
            ShowNetworkedNotification("WARNING: THE SUSPECT HAS A 25 SECOND INVINCIBILITY SHIELD! KEEP YOUR DISTANCE!",
                "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "Notification", 33f);
            ShowDialog("Screw you all!", 5000, 10f);

            EventSequence(player);
        }

        private async void EventSequence(Ped player)
        {
            suspect.IsInvincible = true;
            await BaseScript.Delay(25000);
            suspect.IsInvincible = false;
            suspect.CanSufferCriticalHits = false;
            suspect.CanBeTargetted = false;
            ShowNetworkedNotification("SHIELD DISABLED!",
                "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "Notification", 25f);
            API.RequestNamedPtfxAsset("core");
            API.UseParticleFxAssetNextCall("core");
            var particle = API.StartNetworkedParticleFxLoopedOnEntity("exp_air_molotov_lod", suspect.GetHashCode(), 0, 0, 0, 0, 0, 0,
                45F, true, true, true);
            var particle2 = API.StartNetworkedParticleFxLoopedOnEntity("exp_grd_rpg_post", suspect.GetHashCode(), 0, 0, 0, 0, 0, 0, 25F,
                true, true, true);
            API.UseParticleFxAssetNextCall("core");
            await BaseScript.Delay(500);
            var particle3 = API.StartNetworkedParticleFxLoopedOnEntity("exp_air_molotov_lod", suspect.GetHashCode(), 0, 0, 0, 0, 0, 0,
                45F, true, true, true);

            TeleportAround(player, particle, particle2);
        }

        private async void TeleportAround(Ped player, int particle, int particle2)
        {
            Random r = new Random();
            while (suspect.IsAlive && suspect.IsInCombatAgainst(player))
            {
                suspect.Position = new Vector3(suspect.Position.X + r.Next(4,5), suspect.Position.Y, suspect.Position.Z);
                await BaseScript.Delay(1200);
                if (!suspect.IsInCombatAgainst(player)) break;
                suspect.Position = new Vector3(suspect.Position.X + r.Next(2,3), suspect.Position.Y, suspect.Position.Z);
                await BaseScript.Delay(1200);
                if (!suspect.IsInCombatAgainst(player)) break;
                suspect.Position = new Vector3(suspect.Position.X, suspect.Position.Y + r.Next(4,5), suspect.Position.Z);
                await BaseScript.Delay(1200);
                suspect.Position = new Vector3(suspect.Position.X, suspect.Position.Y + r.Next(2,3), suspect.Position.Z);
            }
            /*API.StopParticleFxLooped(particle, true);
            API.StopParticleFxLooped(particle2, true);*/
        }
    }
}