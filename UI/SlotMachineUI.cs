using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using System.Linq;
using Terraria.ID;
using System;
using Terraria.DataStructures;
using ReLogic.Content;
using Terraria.Audio;
using SlotMachine;
using System.Threading.Tasks;
using Terraria.GameContent;

namespace SlotMachineItem.UI
{
	public class SlotMachineUI : UIState
	{
		private UIPanel _container;
		private UIImage SlotMachineImage;
		private int? playerBet;
		private int betAmount = 1;
		private bool _isVisible = false;
		private double _lastGambleTime = 0;
		private bool _isClicking = false;
		public bool IsActive => _isVisible;
		private SlotMachineWheel _wheel1;
		private SlotMachineWheel _wheel2;
		private SlotMachineWheel _wheel3;
		private bool _isSpinning = false;
		private SoundStyle _winSound, _startSound;
		private string _tooltipText;
		private Asset<Texture2D> _originalTexture;
		private Asset<Texture2D> _gamblingTexture;

		public override void OnInitialize()
		{
			if (Main.LocalPlayer.whoAmI != Main.myPlayer)
			{
				return;
			}

			_winSound = new SoundStyle("SlotMachine/Sounds/WinSound");

			_container = new UIPanel();
			_container.Width.Set(268f, 0f);
			_container.Height.Set(298f, 0f);
			_container.BackgroundColor = new Color(0, 0, 0, 0);
			_container.BorderColor = new Color(0, 0, 0, 0);

			_originalTexture = ModContent.Request<Texture2D>("SlotMachine/Textures/UI/SlotMachine");
			_gamblingTexture = ModContent.Request<Texture2D>("SlotMachine/Textures/UI/SlotMachineState2");

			SlotMachineImage = new UIImage(_originalTexture.Value);
			SlotMachineImage.Width.Set(264f, 0f);
			SlotMachineImage.Height.Set(294f, 0f);

			Asset<Texture2D> wheelTexture = ModContent.Request<Texture2D>("SlotMachine/Textures/UI/SlotMachineWheel");

			_wheel1 = new SlotMachineWheel(wheelTexture.Value, 3);
			_wheel2 = new SlotMachineWheel(wheelTexture.Value, 3);
			_wheel3 = new SlotMachineWheel(wheelTexture.Value, 3);

			_wheel1.Left.Set(47f, 0f);
			_wheel1.Top.Set(69f, 0f);
			_wheel2.Left.Set(95f, 0f);
			_wheel2.Top.Set(69f, 0f);
			_wheel3.Left.Set(143f, 0f);
			_wheel3.Top.Set(69f, 0f);

			_container.Append(_wheel1);
			_container.Append(_wheel2);
			_container.Append(_wheel3);

			SlotMachineImage.Left.Set(-10f, 0f);
			SlotMachineImage.Top.Set(-10f, 0f);
			_container.Append(SlotMachineImage);

			Append(_container);

			this.Hide();
		}

		public void Show()
		{
			_isVisible = true;

			float uiScale = Main.UIScale;
			float centerX = (Main.screenWidth - (_container.Width.Pixels * uiScale)) / 2;
			float centerY = (Main.screenHeight - (_container.Height.Pixels * uiScale)) / 2;

			_container.Left.Set(centerX / uiScale, 0f);
			_container.Top.Set(centerY / uiScale, 0f);
			_container.Recalculate();
		}

		public void Hide()
		{
			_isVisible = false;
			this.Remove();
		}

		public void StartGambling()
		{
			if (_isSpinning)
			{
				return;
			}

			Player player = Main.LocalPlayer;
			int playerGoldCoins = player.inventory.Where(item => item.type == ItemID.GoldCoin).Sum(item => item.stack);
			int playerPlatinumCoins = player.inventory.Where(item => item.type == ItemID.PlatinumCoin).Sum(item => item.stack);
			int totalGoldValue = playerGoldCoins + playerPlatinumCoins * 100;

			if (totalGoldValue < betAmount)
			{
				return;
			}

			AdjustPlayerGoldCoins(player, -betAmount);

			_startSound = new SoundStyle("SlotMachine/Sounds/StartSound");
			SoundEngine.PlaySound(_startSound);

			SlotMachineImage.SetImage(_gamblingTexture.Value);  // Changes texture to gambling state

			StartSpinWithDelay();
		}

		private async void StartSpinWithDelay()
		{
			await Task.Delay(400);
			SlotMachineImage.SetImage(_originalTexture.Value);
			StartSpin();
		}

		public void StartSpin()
		{
			_isSpinning = true;
			_wheel1.StartSpin(3f);
			_wheel2.StartSpin(4f);

			if (_wheel1.FinalSymbol == _wheel2.FinalSymbol)
			{
				_wheel3.StartSpin(7f);
			}
			else
			{
				_wheel3.StartSpin(5f);
			}
		}

		public void StopSpin()
		{
			_isSpinning = false;
			SlotMachineImage.SetImage(_originalTexture.Value);
		}

		public bool IsMouseOverUI()
		{
			return SlotMachineImage.IsMouseHovering;
		}

		private void AdjustBetAmount(int amount)
		{
			betAmount = Math.Max(1, betAmount + amount);
		}

		private void SetMaxBetAmount()
		{
			Player player = Main.LocalPlayer;
			int playerGoldCoins = player.inventory.Where(item => item.type == ItemID.GoldCoin).Sum(item => item.stack);
			int playerPlatinumCoins = player.inventory.Where(item => item.type == ItemID.PlatinumCoin).Sum(item => item.stack);
			int totalGoldValue = playerGoldCoins + playerPlatinumCoins * 100;

			betAmount = Math.Max(1, totalGoldValue);
		}

		private void AdjustPlayerGoldCoins(Player player, int amount)
		{
			IEntitySource entitySource = player.GetSource_FromThis();

			if (amount > 0)
			{
				int platinumToAdd = amount / 100;
				int goldToAdd = amount % 100;

				if (platinumToAdd > 0)
				{
					player.QuickSpawnItem(entitySource, ItemID.PlatinumCoin, platinumToAdd);
				}
				if (goldToAdd > 0)
				{
					player.QuickSpawnItem(entitySource, ItemID.GoldCoin, goldToAdd);
				}
			}
			else
			{
				int remainingAmount = -amount;
				for (int i = 0; i < player.inventory.Length; i++)
				{
					if (remainingAmount <= 0) break;

					if (player.inventory[i] != null && player.inventory[i].type == ItemID.PlatinumCoin)
					{
						int platinumValue = player.inventory[i].stack * 100;
						if (platinumValue > remainingAmount)
						{
							int platinumToRemove = remainingAmount / 100;
							int goldToRemove = remainingAmount % 100;

							player.inventory[i].stack -= platinumToRemove;
							remainingAmount -= platinumToRemove * 100;

							if (goldToRemove > 0)
							{
								player.inventory[i].stack -= 1;
								remainingAmount -= 100;
								player.QuickSpawnItem(entitySource, ItemID.GoldCoin, 100 - goldToRemove);
							}
						}
						else
						{
							remainingAmount -= platinumValue;
							player.inventory[i].SetDefaults(0);
						}
					}
					else if (player.inventory[i] != null && player.inventory[i].type == ItemID.GoldCoin)
					{
						if (player.inventory[i].stack > remainingAmount)
						{
							player.inventory[i].stack -= remainingAmount;
							remainingAmount = 0;
						}
						else
						{
							remainingAmount -= player.inventory[i].stack;
							player.inventory[i].SetDefaults(0);
						}
					}
				}

				while (remainingAmount > 0)
				{
					bool converted = false;
					for (int i = 0; i < player.inventory.Length; i++)
					{
						if (player.inventory[i] != null && player.inventory[i].type == ItemID.PlatinumCoin)
						{
							int platinumValue = player.inventory[i].stack * 100;
							if (platinumValue > remainingAmount)
							{
								int platinumToRemove = remainingAmount / 100;
								int goldToRemove = remainingAmount % 100;

								player.inventory[i].stack -= platinumToRemove;
								remainingAmount -= platinumToRemove * 100;

								if (goldToRemove > 0)
								{
									player.inventory[i].stack -= 1;
									remainingAmount -= 100;
									player.QuickSpawnItem(entitySource, ItemID.GoldCoin, 100 - goldToRemove);
								}
								converted = true;
								break;
							}
							else
							{
								remainingAmount -= platinumValue;
								player.inventory[i].SetDefaults(0);
								converted = true;
								break;
							}
						}
					}
					if (!converted) break;
				}
			}
		}

		private void CheckWinCondition()
		{
			int sevenSymbolIndex = 2, cherrySymbolIndex = 1;
			int[] lemonSymbolIndices = { 0, 3 };
			int[] pineappleSymbolIndices = { 4, 5 };

			if (_wheel1.FinalSymbol == _wheel2.FinalSymbol && _wheel2.FinalSymbol == _wheel3.FinalSymbol)
			{
				if (_wheel1.FinalSymbol == sevenSymbolIndex)
				{
					AdjustPlayerGoldCoins(Main.LocalPlayer, betAmount * 50);
					Vector2 center = Main.LocalPlayer.Center;
					SpawnRoundParticles(center + new Vector2(-250, -200), 1000, 50f);
					SpawnRoundParticles(center + new Vector2(0, -300), 1000, 50f);
					SpawnRoundParticles(center + new Vector2(250, -200), 1000, 50f);
				}
				else if (_wheel1.FinalSymbol == cherrySymbolIndex)
				{
					AdjustPlayerGoldCoins(Main.LocalPlayer, betAmount * 10);
					SpawnParticles(1000, 1.0f, Main.LocalPlayer.Center);
				}
				else if (lemonSymbolIndices.Contains(_wheel1.FinalSymbol) || pineappleSymbolIndices.Contains(_wheel1.FinalSymbol))
				{
					AdjustPlayerGoldCoins(Main.LocalPlayer, betAmount * 3);
					SpawnParticles(1000, 1.0f, Main.LocalPlayer.Center);
				}
				SoundEngine.PlaySound(_winSound);
			}
			else
			{
				AdjustPlayerGoldCoins(Main.LocalPlayer, -betAmount);
			}
		}

		private void SpawnParticles(int numberOfParticles, float duration, params Vector2[] positions)
		{
			int[] dustTypes = { DustID.Firework_Red, DustID.Firework_Blue, DustID.Firework_Green, DustID.Firework_Yellow };
			SoundEngine.PlaySound(SoundID.Item14);
			foreach (var position in positions)
			{
				for (int i = 0; i < numberOfParticles; i++)
				{
					Vector2 offset = new Vector2(Main.rand.NextFloat(-50, 50), Main.rand.NextFloat(-50, 50));
					Vector2 particlePosition = position + offset;
					int dustType = dustTypes[Main.rand.Next(dustTypes.Length)]; // Randomly select a dust type
					Dust dust = Dust.NewDustDirect(particlePosition, 10, 10, dustType, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
					dust.noGravity = true;
					dust.scale = 1.5f;
				}
			}
		}

		private void SpawnRoundParticles(Vector2 center, int numberOfParticles, float radius)
		{
			int[] dustTypes = { DustID.Firework_Red, DustID.Firework_Blue, DustID.Firework_Green, DustID.Firework_Yellow };
			SoundEngine.PlaySound(SoundID.Item14, center);
			for (int i = 0; i < numberOfParticles; i++)
			{
				float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
				Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
				Vector2 particlePosition = center + offset;
				int dustType = dustTypes[Main.rand.Next(dustTypes.Length)]; // Randomly select a dust type
				Dust dust = Dust.NewDustDirect(particlePosition, 10, 10, dustType, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
				dust.noGravity = true;
				dust.scale = 1.5f;
			}
		}

		private int GetMouseOverArea()
		{
			Vector2 mousePosition = new Vector2(Main.mouseX, Main.mouseY);
			CalculatedStyle dimensions = _container.GetDimensions();

			float areaWidth = 20;
			float areaLeft1 = dimensions.X + 59;
			float areaRight1 = areaLeft1 + areaWidth;
			float areaLeft2 = areaRight1 + 8;
			float areaRight2 = areaLeft2 + areaWidth;
			float areaLeft3 = areaRight2 + 5;
			float areaRight3 = areaLeft3 + areaWidth;
			float areaTop = dimensions.Y + 206;
			float areaBottom = dimensions.Y + dimensions.Height - 77;

			if (mousePosition.X >= areaLeft1 && mousePosition.X <= areaRight1 &&
				mousePosition.Y >= areaTop && mousePosition.Y <= areaBottom)
			{
				return 1;
			}
			else if (mousePosition.X >= areaLeft2 && mousePosition.X <= areaRight2 &&
					 mousePosition.Y >= areaTop && mousePosition.Y <= areaBottom)
			{
				return 2;
			}
			else if (mousePosition.X >= areaLeft3 && mousePosition.X <= areaRight3 &&
					 mousePosition.Y >= areaTop && mousePosition.Y <= areaBottom)
			{
				return 3;
			}

			return 0;
		}

		public void HandleMouseClick()
		{
			int area = GetMouseOverArea();
			if (_isSpinning)
			{
				_tooltipText = "You cannot change the bet amount while gambling.";
				return;
			}

			switch (area)
			{
				case 1:
					SetMaxBetAmount();
					break;
				case 2:
					AdjustBetAmount(betAmount);
					break;
				case 3:
					AdjustBetAmount(-betAmount / 2);
					break;
				default:
					StartGambling();
					break;
			}
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (_isSpinning && !_wheel1.IsSpinning && !_wheel2.IsSpinning && !_wheel3.IsSpinning)
			{
				StopSpin();
				CheckWinCondition();
			}

			Player player = Main.LocalPlayer;
			int playerGoldCoins = player.inventory.Where(item => item.type == ItemID.GoldCoin).Sum(item => item.stack);
			int playerPlatinumCoins = player.inventory.Where(item => item.type == ItemID.PlatinumCoin).Sum(item => item.stack);
			int totalGoldValue = playerGoldCoins + playerPlatinumCoins * 100;

			int hoveredArea = GetMouseOverArea();
			if (_isSpinning & IsMouseOverUI() & hoveredArea == 0)
			{
				_tooltipText = "Mod by Martin";
			}
			else if (hoveredArea != 0)
			{
				if (_isSpinning)
				{
					_tooltipText = "You cannot change the bet amount while gambling.";
				}
				else
				{
					switch (hoveredArea)
					{
						case 1:
							_tooltipText = $"Max Bet\nCurrent Bet: {betAmount} Gold";
							break;
						case 2:
							_tooltipText = $"Double Bet\nCurrent Bet: {betAmount} Gold";
							break;
						case 3:
							_tooltipText = $"Half Bet\nCurrent Bet: {betAmount} Gold";
							break;
					}
				}
			}
			else if (IsMouseOverUI())
			{
				if (totalGoldValue < betAmount)
				{
					_tooltipText = "You don't have enough money to spin.";
				}
				else
				{
					_tooltipText = "Click to Spin!";
				}
			}
			else
			{
				_tooltipText = null;
			}

			if (Main.mouseLeft && Main.mouseLeftRelease)
			{
				HandleMouseClick();
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!_isVisible)
			{
				return;
			}
			base.Draw(spriteBatch);

			if (!string.IsNullOrEmpty(_tooltipText))
			{
				Vector2 mousePosition = new Vector2(Main.mouseX, Main.mouseY);
				Vector2 textSize = FontAssets.MouseText.Value.MeasureString(_tooltipText);
				Vector2 textPosition = mousePosition + new Vector2(16f, 16f); // offset text from mouse cursor

				// background for tooltip
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)textPosition.X - 4, (int)textPosition.Y - 4, (int)textSize.X + 8, (int)textSize.Y + 8), Color.Black * 0.75f);

				Color textColor = Color.White;
				if (_tooltipText.Contains("Click to Spin!"))
				{
					textColor = Color.Cyan;
				}
				else if (_tooltipText.Contains("You cannot change the bet amount while gambling.") || _tooltipText.Contains("You don't have enough money to spin."))
				{
					textColor = Color.Red;
				}
				else
				{
					textColor = Color.White;
				}

				Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, _tooltipText, textPosition.X, textPosition.Y, textColor, Color.Black, Vector2.Zero);
			}
		}
	}
}