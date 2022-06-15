using BerryLoaderNS;
using BepInEx;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace YourModNS
{
	class Main : BerryLoaderMod
	{
		public override void Init()
		{
			BerryLoader.L.LogInfo("hello from YourMod.Main.Init");
		}
	}

	class Composter : GameCard
	{
		protected override void Awake()
		{
			ReflectionHelper.CopyGameCardProps(this, BerryLoader.tempCurrentGameCard);
			base.Awake();
		}

		protected override void Update()
		{
			base.Update();
			this.CardData.Update();
		}
	}

	class Miku : GameCard
	{
		protected override void Awake()
		{
			ReflectionHelper.CopyGameCardProps(this, BerryLoader.tempCurrentGameCard);
			base.Awake();
			BerryLoader.L.LogInfo("MIKU TIME");
			StartCoroutine(Sing());
		}

		protected override void Update()
		{
			base.Update();
			this.CardData.Update();
		}


		IEnumerator Sing()
		{
			while (true)
			{
				base.RotWobble(1f);
				yield return new WaitForSeconds(2f);
			}
		}
	}

	class Silo : CardData
	{
		[ExtraData("holding_id")]
		public string holdingId;
		[ExtraData("holding_name")]
		public string holdingName;
		[ExtraData("hold_count")]
		public int holdCount;
		public int maxCount = 50;

		protected override bool CanHaveCard(CardData otherCard) => holdingId.Equals("") ? true : otherCard.Id == holdingId;

		public override void Update()
		{
			if (this.holdCount == 0)
			{
				this.holdingId = "";
				this.holdingName = "";
			}
			foreach (GameCard childCard in this.MyGameCard.GetChildCards())
			{
				BerryLoader.L.LogInfo(childCard.CardData.Id);
				if (childCard.CardData.Id == holdingId || holdingId.Equals(""))
				{
					if (this.holdCount < maxCount)
					{
						childCard.DestroyCard(true);
						++this.holdCount;
						this.holdingId = childCard.CardData.Id;
						this.holdingName = childCard.CardData.Name;
					}
				}
				else
					childCard.RemoveFromStack();
			}
			this.gameObject.GetComponent<ModOverride>().Description = $"Holding {this.holdCount}/{this.maxCount} {this.holdingName}";
			base.Update();
		}

		public override void Clicked()
		{
			if (this.holdCount > 0)
			{
				var amount = Mathf.Min(5, holdCount);
				GameCard cards = CreateCards(this.transform.position + Vector3.up * 0.2f, holdingId, amount, false);
				WorldManager.instance.StackSend(cards.GetRootCard());
				this.holdCount -= amount;
				if (holdCount == 0)
					this.holdingId = "";
			}
		}

		GameCard CreateCards(Vector3 pos, string id, int amount, bool checkAddToStack = true)
		{
			if (amount == 0)
				return (GameCard)null;
			GameCard gold = (GameCard)null;
			int num;
			for (; amount > 0; amount -= num)
			{
				num = Mathf.Min(amount, 30);
				gold = (GameCard)null;
				for (int index = 0; index < num; ++index)
				{
					GameCard gameCard = WorldManager.instance.CreateCard(pos, id, checkAddToStack: checkAddToStack).MyGameCard;
					if ((UnityEngine.Object)gold != (UnityEngine.Object)null)
					{
						gameCard.Parent = gold;
						gold.Child = gameCard;
					}
					gold = gameCard;
				}
			}
			return gold;
		}
	}
}