using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;

namespace GAMCallouts
{
    [CalloutProperties(name: "Humane Labs Raid", author: "GAM Studios", version: "0.0.1")] //boxville3 is humane van
    public class HumaneRaid : Callout
    {
        private Ped[] suspects = new Ped[16];
        private Vehicle[] cars = new Vehicle[2];
        private Ped spec1, spec2;
        private Vector3[] coordinates =
        {
            new Vector3(3621f, 3732f, 29f),
            new Vector3(3609f, 3744f, 29f),
            new Vector3(3614f, 3744f, 29f),
            new Vector3(3618f, 3744f, 29f),
            new Vector3(3610f, 3713f, 30f),
            new Vector3(3603f, 3722f, 30f),
            new Vector3(3595f, 3715f, 30f),
            new Vector3(3593f, 3705f, 30f),
            new Vector3(3595f, 3693f, 29f),
            new Vector3(3589f, 3686f, 28f),
            new Vector3(3568f, 3697f, 29f),
            new Vector3(3560f, 3682f, 29f),
            new Vector3(3551f, 3663f, 29f),
            new Vector3(3545f, 3642f, 29f),
            new Vector3(3531f, 3668f, 29f),
            new Vector3(3572f, 3673f, 29f)
        };

        private PedHash[] suspects_possible =
        {
            PedHash.Ballasog,
            PedHash.Ballas01GFY,
            PedHash.BallaEast01GMY,
            PedHash.BallaOrig01GMY,
            PedHash.BallaSout01GMY
        };

        private Vector3 lab_coord = new Vector3(3560.66f, 3674.87f, 29f); //Special neurotoxin lab area
        private Vector3 second_lab_coord = new Vector3(3539f, 3667f, 29f); //Other lab area at end
        public static RelationshipGroup suspectRelations = World.AddRelationshipGroup("PLAYERATTACKER");
        public static RelationshipGroup playerRelations = World.AddRelationshipGroup("PLAYERRELLER");

        public HumaneRaid()
        {
            InitInfo(new Vector3(3438f, 3768f, 31f));
            ShortName = "Humane Labs Raid";
            CalloutDescription = "An armed faction is attempting to steal a potent neurotoxin.";
            ResponseCode = 3;
            StartDistance = 65f;
        }

        public override async Task OnAccept()
        {
            InitBlip();
            WeaponHash[] guns = { WeaponHash.CombatMGMk2, WeaponHash.AdvancedRifle, WeaponHash.AssaultShotgun };
            for (var i = 0; i < suspects.Length; i++)
            {
                suspects[i] = await SpawnPed(suspects_possible[RandomUtils.Random.Next(suspects_possible.Length)], coordinates[i]);
                suspects[i].MaxHealthFloat = 100000f;
                suspects[i].HealthFloat = 10000f;
                suspects[i].ArmorFloat = 3000f;
                suspects[i].AlwaysKeepTask = true;
                suspects[i].BlockPermanentEvents = true;
                suspects[i].Weapons.Give(guns[RandomUtils.Random.Next(guns.Length)], 9999, true, true);
                API.Wait(4);
            }

            spec1 = await SpawnPed(PedHash.Doctor01SMM, lab_coord);
            spec2 = await SpawnPed(PedHash.Doctor01SMM, second_lab_coord);
            spec1.AlwaysKeepTask = true;
            spec1.BlockPermanentEvents = true;
            spec1.Weapons.Give(WeaponHash.RPG, 30, false, true);
            spec2.AlwaysKeepTask = true;
            spec2.BlockPermanentEvents = true;
            spec2.Weapons.Give(WeaponHash.RPG, 30, false, true);
        }

        public override void OnStart(Ped player)
        {
            base.OnStart(player);
            player.RelationshipGroup = playerRelations;
            suspectRelations.SetRelationshipBetweenGroups(playerRelations, Relationship.Hate);

            foreach (var sus in suspects)
            {
                sus.Accuracy = 99;
                sus.FiringPattern = FiringPattern.FullAuto;
                sus.Task.FightAgainst(player);
                sus.RelationshipGroup = suspectRelations;
                API.Wait(2);
            }

            spec1.Accuracy = 99;
            spec1.FiringPattern = FiringPattern.FullAuto;
            spec1.Task.GuardCurrentPosition();
            spec1.Task.FightAgainst(player);
            
            spec1.RelationshipGroup = suspectRelations;
            
            spec2.Accuracy = 99;
            spec2.FiringPattern = FiringPattern.FullAuto;
            spec2.Task.GuardCurrentPosition();
            spec2.Task.FightAgainst(player);

            spec2.RelationshipGroup = suspectRelations;
        }

    }
}