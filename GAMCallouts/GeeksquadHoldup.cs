using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;

namespace GAMCallouts
{
    [CalloutProperties(name: "Geeksquad Hold-Up", author: "GAM Studios", version: "0.0.1")]
    public class GeeksquadHoldup : Callout
    {
        private Ped[] suspects = new Ped[4];
        private Vehicle car, car2;
        private Ped victim;
        
        public static RelationshipGroup suspectRelations = World.AddRelationshipGroup("PLAYERATTACKER");
        public static RelationshipGroup playerRelations = World.AddRelationshipGroup("PLAYERRELLER");

        public GeeksquadHoldup()
        {
            Random r = new Random();
            float offsetX = r.Next(100, 700);
            float offsetY = r.Next(100, 700);
            InitInfo(World.GetNextPositionOnStreet(Game.PlayerPed.GetOffsetPosition(new Vector3(offsetX, offsetY, 0))));
            ShortName = "Geeksquad Hold-Up";
            CalloutDescription = "Geeksquad seems to be up to something. Find out what is going on.";
            ResponseCode = 2;
            StartDistance = 65f;
        }

        public override async Task OnAccept()
        {
            suspects[0] = await SpawnPed(PedHash.Business02AMY, Location + 3);
            suspects[1] = await SpawnPed(PedHash.TaoCheng, Location + 4);
            suspects[2] = await SpawnPed(PedHash.GentransportSMM, Location - 2);
            suspects[3] = await SpawnPed(PedHash.Barman01SMY, Location - 4);
            victim = await SpawnPed(RandomUtils.GetRandomPed(), Location - 1);
            car = await SpawnVehicle(VehicleHash.Rumpo2, Location);
            car2 = await SpawnVehicle(VehicleHash.Rumpo2, Location + 7);
            car.Mods.PrimaryColor = VehicleColor.MetallicBlack;
            car2.Mods.PrimaryColor = VehicleColor.MetallicBlack;

            suspects[0].SetIntoVehicle(car, VehicleSeat.Driver);
            suspects[1].SetIntoVehicle(car, VehicleSeat.Passenger);
            suspects[2].SetIntoVehicle(car2, VehicleSeat.Driver);
            suspects[3].SetIntoVehicle(car2, VehicleSeat.Passenger);
            for (int i = 0; i < suspects.Length; i++)
            {
                suspects[i].AlwaysKeepTask = true;
                suspects[i].BlockPermanentEvents = true;
            }

            car.AttachBlip();
            car2.AttachBlip();
            suspects[2].VehicleDrivingFlags = VehicleDrivingFlags.IgnorePathFinding;
            UpdateData();
        }

        public override void OnStart(Ped player)
        {
            base.OnStart(player);
            ShowNetworkedNotification("Geeksquad seems to be up to something. Find out what is going on.",
                "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "Notification", 33f);
            EventSequence(player);
        }

        public async void EventSequence(Ped player)
        {
            foreach (var suspect in suspects)
            {
                suspect.Weapons.Give(WeaponHash.MiniSMG, 9999, true, true);
                suspect.MaxHealthFloat = 100000f;
                suspect.HealthFloat = 10000f;
                suspect.ArmorFloat = 3000f;
                suspect.FiringPattern = FiringPattern.FullAuto;
                suspect.Accuracy = 99;
            }

            Utilities.ExcludeVehicleFromTrafficStop(car.NetworkId, true);
            Utilities.ExcludeVehicleFromTrafficStop(car2.NetworkId, true);
            VehicleData vehicleData1 = await Utilities.GetVehicleData(car.NetworkId);
            VehicleData vehicleData2 = await Utilities.GetVehicleData(car2.NetworkId);

            PedData pedData1 = await Utilities.GetPedData(suspects[0].NetworkId);
            PedData pedData2 = await Utilities.GetPedData(suspects[2].NetworkId);
            
            Item pc_eq = new Item
            {
                Name = "PC repair equipment",
                IsIllegal = false
            };

            Item gamer_g = new Item
            {
                Name = "Gamer gunk",
                IsIllegal = true
            };

            Item gs_badge = new Item
            {
                Name = "Geeksquad badge",
                IsIllegal = false
            };
            
            vehicleData1.Items.Add(pc_eq);
            vehicleData2.Items.Add(pc_eq);
            pedData1.Items.Add(gamer_g);
            pedData2.Items.Add(gamer_g);
            pedData1.Items.Add(gs_badge);
            pedData2.Items.Add(gs_badge);

            Utilities.SetVehicleData(car.NetworkId, vehicleData1);
            Utilities.SetVehicleData(car2.NetworkId, vehicleData2);
            Utilities.SetPedData(suspects[0].NetworkId, pedData1);
            Utilities.SetPedData(suspects[2].NetworkId, pedData2);
            
            suspects[0].Task.DriveTo(car, victim.GetPositionOffset(new Vector3(2f, 2f, 0f)), 3f, 60, (int) DrivingStyle.Rushed);
            ShowDialog("Hey you, need your PC fixed? We've got a special discount!", 8000, 60f);
            ShowNetworkedNotification("Hey you, need your PC fixed? We've got a special discount!",
                "CHAR_BLANK_ENTRY", "CHAR_BLANK_ENTRY", "Geeksquad", "Dialogue", 39f);
            await BaseScript.Delay(6000);
            
            ShowDialog("Well I guess not then!", 8000, 60f);
            ShowNetworkedNotification("Well I guess not then!",
                "CHAR_BLANK_ENTRY", "CHAR_BLANK_ENTRY", "Geeksquad", "Dialogue", 39f);
            suspects[0].Task.VehicleShootAtPed(victim);
            suspects[1].Task.VehicleShootAtPed(victim);
            suspects[3].Task.VehicleShootAtPed(victim);
            victim.Task.ReactAndFlee(suspects[0]);

            await BaseScript.Delay(1000);
            
            suspects[0].Task.ClearAll();
            suspects[0].Task.FleeFrom(player);
            API.TaskVehicleFollow(suspects[2].GetHashCode(), car2.GetHashCode(), car.GetHashCode(), 80f, (int)DrivingStyle.Rushed,
                6);
            
            player.RelationshipGroup = playerRelations;
            suspectRelations.SetRelationshipBetweenGroups(playerRelations, Relationship.Hate);
            
            suspects[0].RelationshipGroup = suspectRelations;
            suspects[1].RelationshipGroup = suspectRelations;
            suspects[2].RelationshipGroup = suspectRelations;
            suspects[3].RelationshipGroup = suspectRelations;
            
            suspects[1].Task.ClearAll();
            suspects[3].Task.ClearAll();
            suspects[1].Task.VehicleShootAtPed(player);
            suspects[3].Task.VehicleShootAtPed(player);

        }
    }
}