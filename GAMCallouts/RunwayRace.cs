using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;

namespace GAMCallouts
{
    [CalloutProperties(name: "Runway Drag Racer", author: "GAM Studios", version: "0.0.1")]
    public class RunwayRace : Callout
    {
        Ped suspect;
        Vehicle raceveh;

        private readonly Vector3[] coordinates =
        {
            new Vector3(-1283f, -2185f, 13.53f), //North runway end
            new Vector3(-1578.48f, -2747.15f, 13.52f),
            new Vector3(-1655f, -2765f, 14f)
        };

        public RunwayRace()
        {
            InitInfo(new Vector3(-1655f, -2765f, 14f));
            ShortName = "Runway Drag Racer";
            CalloutDescription = "A person in a car is speeding along the runway.";
            ResponseCode = 2;
            StartDistance = 80f;
        }

        public override async Task OnAccept()
        {
            InitBlip();
            UpdateData();
            var carlist = new[]
            {
                VehicleHash.XA21,
                VehicleHash.Adder,
                VehicleHash.Nero,
                VehicleHash.T20,
                VehicleHash.Tyrus
            };
            raceveh = await SpawnVehicle(carlist[RandomUtils.Random.Next(carlist.Length)], Location, 60f);
            raceveh.EnginePowerMultiplier = 150;
            raceveh.EngineTorqueMultiplier = 2;
            suspect = await SpawnPed(RandomUtils.GetRandomPed(), Location);
            suspect.AlwaysKeepTask = true;
            suspect.BlockPermanentEvents = true;
            raceveh.AttachBlip();
            suspect.SetIntoVehicle(raceveh, VehicleSeat.Driver);
            suspect.VehicleDrivingFlags = VehicleDrivingFlags.IgnorePathFinding;
        }

        public override void OnStart(Ped player)
        {
            base.OnStart(player);
            suspect.AttachBlip();
            suspect.MaxDrivingSpeed = int.MaxValue;
            suspect.DrivingSpeed = 900;
            suspect.Task.DriveTo(raceveh, new Vector3(-1283f, -2185f, 13.53f), 150, 600, (int) DrivingStyle.Rushed);
            suspect.Task.FollowPointRoute(coordinates);
            suspect.Task.FleeFrom(player);
            ShowDialog("Gotta go fast!", 5000, 10f);
            
            //RestartLap();
        }

        private async void RestartLap()
        {
            var restartpos = new Vector3(-1661f, -2817f, 14f);
            await BaseScript.Delay(30000);
            if (Vector3.DistanceSquared(restartpos, suspect.Position) < 10f)
            {
                suspect.Task.DriveTo(raceveh, new Vector3(-1652.43f, -2758.48f, 13.94f), 20f, 600f, (int) DrivingStyle.Rushed);
            }
        }
    }
}