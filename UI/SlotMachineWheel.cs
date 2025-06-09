using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using System;
using Terraria.Audio;

namespace SlotMachineItem.UI
{
	public class SlotMachineWheel : UIElement
	{
		private Texture2D _wheelTexture;  // slot symbols sprite sheet
		private int _symbolCount;  // number of symbols
		private int _currentSymbolIndex;  // current symbol index
		private int _symbolHeight;  // height of each symbol
		private int _frameHeight;  // height of visible frame
		private float _spinSpeed;  // current spin speed
		private float _spinTime;  // remaining spin time
		private float _currentOffset;  // offset for smooth spin
		private Random _random;
		private SoundStyle _stopSound;  // sound when wheel stops

		private bool _isSpinning;

		public int FinalSymbol { get; private set; }  // symbol to stop on
		public bool IsSpinning => _isSpinning;  // is wheel spinning

		public SlotMachineWheel(Texture2D wheelTexture, int symbolCount)
		{
			_wheelTexture = wheelTexture;
			_symbolCount = symbolCount;
			_symbolHeight = wheelTexture.Height / symbolCount;  // vertical symbols
			_frameHeight = _symbolHeight + 42;  // padding
			_random = new Random();
		}

		public void StartSpin(float spinTime)
		{
			_spinSpeed = 1000f;
			_spinTime = spinTime;
			_isSpinning = true;
			_currentOffset = 0f;
			FinalSymbol = _random.Next(0, _symbolCount);
		}

		public void StopSpin()
		{
			_stopSound = new SoundStyle("SlotMachine/Sounds/StopSound");
			_isSpinning = false;
			_currentSymbolIndex = FinalSymbol;
			_currentOffset = 0f;
			SoundEngine.PlaySound(_stopSound);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (_isSpinning)
			{
				// update spin time and slow down
				_spinTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (_spinTime <= 0)
				{
					StopSpin();  // stop when time is up
				}
				else
				{
					// update offset by speed
					_currentOffset += _spinSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
					if (_currentOffset >= _symbolHeight)
					{
						_currentOffset -= _symbolHeight;
						_currentSymbolIndex = (_currentSymbolIndex + 1) % _symbolCount;
					}

					// reduce speed to slow down
					_spinSpeed = MathHelper.Max(0.05f, _spinSpeed - 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds);
				}
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);

			// calculate source rectangle for current symbol
			int sourceY = _currentSymbolIndex * _symbolHeight + (int)_currentOffset - 21;
			int sourceHeight = _symbolHeight + 42;

			// if the source rectangle starts above the texture, wrap from the bottom
			if (sourceY < 0)
			{
				int overflow = -sourceY;
				sourceY = _wheelTexture.Height - overflow;
				sourceHeight -= overflow;

				// draw the wrapped part from the bottom of the texture
				var symbolRectangle1 = new Rectangle(0, sourceY, _wheelTexture.Width, overflow);
				var destinationRectangle1 = new Rectangle((int)Math.Round(GetDimensions().X), (int)Math.Round(GetDimensions().Y), _wheelTexture.Width, overflow);
				spriteBatch.Draw(_wheelTexture, destinationRectangle1, symbolRectangle1, Color.White);

				// draw the remaining part from the top
				var symbolRectangle2 = new Rectangle(0, 0, _wheelTexture.Width, sourceHeight);
				var destinationRectangle2 = new Rectangle((int)Math.Round(GetDimensions().X), (int)Math.Round(GetDimensions().Y) + overflow, _wheelTexture.Width, sourceHeight);
				spriteBatch.Draw(_wheelTexture, destinationRectangle2, symbolRectangle2, Color.White);
			}
			// if the source rectangle goes past the bottom, wrap to the top
			else if (sourceY + sourceHeight > _wheelTexture.Height)
			{
				int overflow = (sourceY + sourceHeight) - _wheelTexture.Height;
				sourceHeight -= overflow;

				// draw the visible part from the current position
				var symbolRectangle1 = new Rectangle(0, sourceY, _wheelTexture.Width, sourceHeight);
				var destinationRectangle1 = new Rectangle((int)Math.Round(GetDimensions().X), (int)Math.Round(GetDimensions().Y), _wheelTexture.Width, sourceHeight);
				spriteBatch.Draw(_wheelTexture, destinationRectangle1, symbolRectangle1, Color.White);

				// draw the wrapped part from the top of the texture
				var symbolRectangle2 = new Rectangle(0, 0, _wheelTexture.Width, overflow);
				var destinationRectangle2 = new Rectangle((int)Math.Round(GetDimensions().X), (int)Math.Round(GetDimensions().Y) + sourceHeight, _wheelTexture.Width, overflow);
				spriteBatch.Draw(_wheelTexture, destinationRectangle2, symbolRectangle2, Color.White);
			}
			else
			{
				// draw the symbol normally if no wrapping is needed
				var symbolRectangle = new Rectangle(0, sourceY, _wheelTexture.Width, sourceHeight);
				var destinationRectangle = new Rectangle((int)Math.Round(GetDimensions().X), (int)Math.Round(GetDimensions().Y), _wheelTexture.Width, sourceHeight);
				spriteBatch.Draw(_wheelTexture, destinationRectangle, symbolRectangle, Color.White);
			}
		}
	}
}