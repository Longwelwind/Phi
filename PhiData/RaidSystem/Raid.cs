using System;

namespace PhiClient.RaidSystem
{
	[Serializable]
	public class Raid : IDable
	{
		public int id;

		public User sender;
		public User target;
		public RealmPawn[] initialPawns;
		public RaidState state = RaidState.SENT;

		public Raid(int id, User sender, User target, RealmPawn[] initialPawns)
		{
			this.sender = sender;
			this.target = target;
			this.initialPawns = initialPawns;
		}

		public int getID()
		{
			return this.id;
		}
	}

	public enum RaidState
	{
		SENT,
		RAIDING,
		INTERRUPTED,
		FINISHED
	}

	public class SendRaidPacket: Packet
	{
		public RealmPawn[] pawns;
		public User target;

		public override void Apply(User user, RealmData realmData)
		{
			Raid raid = new Raid(realmData.lastRaidId, user, target, pawns);

			realmData.ServerAddRaid(raid);
		}
	}

	public class NotifyRaid: Packet
	{
		public Raid raid;

		public override void Apply(User user, RealmData realmData)
		{
			realmData.raids.Add(raid);

			if (user == raid.target)
			{
				// TODO: Spawn the raid
			}
		}
	}
}

