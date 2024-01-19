using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;

namespace GAMCallouts
{
    [CalloutProperties(name: "Mountain Dew Bandits", author: "GAM Studios", version: "0.0.1")]
    public class DewBandits : Callout
    {
        private Ped[] suspects = new Ped[16];
        private Vehicle[] cars = new Vehicle[4];
        private Boolean[] suspectDriver = new Boolean[16];
        private int count, j = 0;
        public static RelationshipGroup suspectRelations = World.AddRelationshipGroup("PLAYERATTACKER");
        public static RelationshipGroup playerRelations = World.AddRelationshipGroup("PLAYERRELLER");

        private readonly Vector3[] coordinates =
        {
            new Vector3(-972f, -3128f, 14f), //North runway beginning
            new Vector3(-1134f, -3375f, 14f),
            new Vector3(-1406f, -3221f, 14f)
            //new Vector3(-1736f, -2914f, 14f) //South runway end
        };

        public DewBandits()
        {
            InitInfo(coordinates[RandomUtils.Random.Next(coordinates.Length)]);
            ShortName = "Mountain Dew Bandits";
            CalloutDescription = "A bunch of Mountain Dew Vans with bandits are terrorizing the airport.";
            ResponseCode = 3;
            StartDistance = 80f;
        }

        public override async Task OnAccept()
        {
            InitBlip();
            for (var i = 0; i < suspects.Length; i++)
            {
                suspects[i] = await SpawnPed(RandomUtils.GetRandomPed(), Location + (i/2));
                API.Wait(2);
            }
            cars[0] = await SpawnVehicle(VehicleHash.Paradise, Location);
            cars[1] = await SpawnVehicle(VehicleHash.Paradise, new Vector3(Location.X + 10f, Location.Y + 10f, Location.Z));
            cars[2] = await SpawnVehicle(VehicleHash.Paradise, new Vector3(Location.X - 10f, Location.Y + 10f, Location.Z));
            cars[3] = await SpawnVehicle(VehicleHash.Paradise, new Vector3(Location.X - 10f, Location.Y - 10f, Location.Z));
            cars[0].Mods.SecondaryColor = VehicleColor.MatteLimeGreen;
            cars[1].Mods.SecondaryColor = VehicleColor.MatteLimeGreen;
            cars[2].Mods.SecondaryColor = VehicleColor.MatteLimeGreen;
            cars[3].Mods.SecondaryColor = VehicleColor.MatteLimeGreen;
            for (int i = 0; i < suspects.Length; i++)
            {
                suspects[i].AlwaysKeepTask = true;
                suspects[i].BlockPermanentEvents = true;
                if (i % 4 == 0 && i != 0)
                {
                    ++count;
                }
                suspects[i].SetIntoVehicle(cars[count], VehicleSeat.Any);
            }

            count = 0;
            for (int i = 0; i < suspects.Length; i++)
            {
                if (!suspects[i].IsInVehicle() && count <= 3)
                {
                    suspects[i].SetIntoVehicle(cars[count], VehicleSeat.Driver);
                    suspectDriver[i] = true;
                    ++count;
                }
            }
            cars[0].AttachBlip();
            cars[1].AttachBlip();
            cars[2].AttachBlip();
            cars[3].AttachBlip();
            cars[0].IsInvincible = true;
        }

        public override void OnStart(Ped player)
        {
            base.OnStart(player);
            ShowNetworkedNotification("The suspects may or may not be overtly hostile. Proceed with caution.",
                "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "Notification", 33f);
            EventSequence(player);
        }

        public async void EventSequence(Ped player)
        {
            ShowDialog("Can't never catch us!", 5000, 10f);
            ShowNetworkedNotification("Can't never catch us!",
                "CHAR_BLANK_ENTRY", "CHAR_BLANK_ENTRY", "Dew Bandits", "Message", 33f);
            for (var i = 0; i < suspects.Length; i++)
            {
                if (suspects[i].IsSittingInVehicle(cars[0]) && suspectDriver[i])
                {
                    suspects[i].Task.VehicleChase(player);
                    j = i;
                } else if (suspectDriver[i])
                {
                    suspects[i].Task.VehicleChase(suspects[j]);
                }
            }

            await BaseScript.Delay(15000);
            player.RelationshipGroup = playerRelations;
            ShowDialog("It's Dew or Die time!", 5000, 10f);
            ShowNetworkedNotification("It's Dew or Die time!",
                "CHAR_BLANK_ENTRY", "CHAR_BLANK_ENTRY", "Dew Bandits", "Message", 33f);
            suspectRelations.SetRelationshipBetweenGroups(playerRelations, Relationship.Hate);
            //zzsuspectRelations.SetRelationshipBetweenGroups(suspectRelations, Relationship.Respect);
            foreach (var susp in suspects)
            {
                susp.Task.ClearAll();
                susp.MaxHealthFloat = 100000f;
                susp.HealthFloat = 10000f;
                susp.ArmorFloat = 3000f;
                susp.Task.LeaveVehicle();
                API.Wait(2);
                susp.Task.ClearAll();
                susp.Task.FightAgainst(player);
                susp.Weapons.Give(WeaponHash.Minigun, 9999, true, true);
                susp.FiringPattern = FiringPattern.FullAuto;
                susp.Accuracy = 99;
                susp.ShootRate = 1000;
                susp.RelationshipGroup = suspectRelations;
            }

            count = 0;
            for (int i = 0; i < suspects.Length; i++)
            {
                API.TaskLeaveVehicle(suspects[i].NetworkId, API.GetVehiclePedIsIn(suspects[i].NetworkId, false), 256);
                suspects[i].Task.FightAgainst(player);
            }
        }
    }
}