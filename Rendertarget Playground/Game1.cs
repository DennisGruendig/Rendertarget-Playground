using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Rendertarget_Playground
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        SpriteFont Font;
        Texture2D Texture;
        Texture2D Projectile;
        Texture2D ProjectileRed;
        Texture2D ProjectileGreen;
        Texture2D ProjectileBlue;
        Texture2D ProjectileLightBlue;
        RenderTarget2D RenderTarget;
        MouseState MouseState;
        KeyboardState KeyboardState;
        List<Vector2> ProjectileLocations = new();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("Font");
            Texture = Content.Load<Texture2D>("GameArea");

            Projectile = new Texture2D(GraphicsDevice, 1, 1);
            ProjectileRed = new Texture2D(GraphicsDevice, 1, 1);
            ProjectileGreen = new Texture2D(GraphicsDevice, 1, 1);
            ProjectileBlue = new Texture2D(GraphicsDevice, 1, 1);
            ProjectileLightBlue = new Texture2D(GraphicsDevice, 1, 1);

            Projectile.SetData(new Color[] { Color.Orange });
            ProjectileRed.SetData(new Color[] { Color.Red });
            ProjectileGreen.SetData(new Color[] { Color.Lime });
            ProjectileBlue.SetData(new Color[] { Color.Blue });
            ProjectileLightBlue.SetData(new Color[] { Color.LightBlue });

            RenderTarget = new RenderTarget2D(GraphicsDevice, Texture.Width, Texture.Height);
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState = Mouse.GetState();
            KeyboardState = Keyboard.GetState();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Should probably only be done on resize
            _graphics.PreferredBackBufferWidth = GraphicsDevice.Viewport.Width;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.Viewport.Height;
            _graphics.ApplyChanges();

            GraphicsDevice.Clear(Color.Magenta);

            Rectangle backBounds = GraphicsDevice.PresentationParameters.Bounds;
            Rectangle targetBounds = RenderTarget.Bounds;

            float backAspect = (float)backBounds.Width / backBounds.Height;
            float targetAspect = (float)targetBounds.Width / targetBounds.Height;

            float targetX = 0;
            float targetY = 0;
            float targetWidth = backBounds.Width;
            float targetHeight = backBounds.Height;

            if (backAspect > targetAspect)
            {
                targetWidth = targetHeight * targetAspect;
                targetX = (backBounds.Width - targetWidth) / 2f;
            }
            else if (backAspect < targetAspect)
            {
                targetHeight = targetWidth / targetAspect;
                targetY = (backBounds.Height - targetHeight) / 2f;
            }

            Vector2 mouseOffset = new Vector2(-targetX, -targetY);

            bool clipToScreen = false;
            float relativeMouseX = 0;
            float relativeMouseY = 0;

            if (clipToScreen)
            {
                if (MouseState.X - targetX > 0)
                    relativeMouseX = ((targetBounds.Width / (backBounds.Width - (2 * targetX)) * (MouseState.X - targetX)));
                if (relativeMouseX >= targetBounds.Width)
                    relativeMouseX = targetBounds.Width;

                if (MouseState.Y - targetY > 0)
                    relativeMouseY = ((targetBounds.Height / (backBounds.Height - (2 * targetY)) * (MouseState.Y - targetY)));
                if (relativeMouseY >= targetBounds.Height)
                    relativeMouseY = targetBounds.Height;
            }
            else
            {
                relativeMouseX = ((targetBounds.Width / (backBounds.Width - (2 * targetX)) * (MouseState.X - targetX)));
                relativeMouseY = ((targetBounds.Height / (backBounds.Height - (2 * targetY)) * (MouseState.Y - targetY)));
            }

            Vector2 centerOfScreen = targetBounds.Size.ToVector2() * 0.5f;
            Vector2 relativeMouse = new Vector2(relativeMouseX, relativeMouseY);
            Vector2 mouseFromMiddle = relativeMouse - centerOfScreen;
            Vector2 compassIndicator = relativeMouse;

            Vector2 normalizedMiddle = mouseFromMiddle;
            normalizedMiddle.Normalize();

            float angle = MathF.Atan2(normalizedMiddle.Y, normalizedMiddle.X) + MathHelper.ToRadians(180);
            int octant = (int)(Math.Round(8 * angle / (2 * MathHelper.Pi) + 8) % 8);

            CompassDir dir = (CompassDir)octant;  // typecast to enum: 0 -> E etc.
            string directionalMessage = string.Empty;

            Vector2 norm = Vector2.Zero;
            switch (dir)
            {
                default: break;
                case CompassDir.N:
                    directionalMessage = "North";
                    compassIndicator = new Vector2(centerOfScreen.X, relativeMouseY);
                    break;

                case CompassDir.E:
                    directionalMessage = "East";
                    compassIndicator = new Vector2(relativeMouseX, centerOfScreen.Y);
                    break;

                case CompassDir.S:
                    directionalMessage = "South";
                    compassIndicator = new Vector2(centerOfScreen.X, relativeMouseY);
                    break;

                case CompassDir.W:
                    directionalMessage = "West";
                    compassIndicator = new Vector2(relativeMouseX, centerOfScreen.Y);
                    break;

                case CompassDir.NE:
                    directionalMessage = "North East";
                    norm = Vector2.Normalize(new Vector2(1, -1));
                    compassIndicator = mouseFromMiddle.Length() * norm + centerOfScreen;
                    break;

                case CompassDir.SE:
                    directionalMessage = "South East";
                    norm = Vector2.Normalize(new Vector2(1, 1));
                    compassIndicator = mouseFromMiddle.Length() * norm + centerOfScreen;
                    break;

                case CompassDir.SW:
                    directionalMessage = "South West";
                    norm = Vector2.Normalize(new Vector2(-1, 1));
                    compassIndicator = mouseFromMiddle.Length() * norm + centerOfScreen;
                    break;

                case CompassDir.NW:
                    directionalMessage = "North West";
                    norm = Vector2.Normalize(new Vector2(-1, -1));
                    compassIndicator = mouseFromMiddle.Length() * norm + centerOfScreen;
                    break;
            }

            float projectileSize = 2;
            float tOffset = (float)(gameTime.TotalGameTime.TotalSeconds * 5 % 1);
            float lineLength = mouseFromMiddle.Length();
            float segmentLength = 3 * projectileSize;
            float nextLength = segmentLength * tOffset;

            ProjectileLocations.Clear();
            while (nextLength <= lineLength)
            {
                ProjectileLocations.Add(centerOfScreen + normalizedMiddle * nextLength);
                nextLength += segmentLength;
            }

            float pointDistance = 250;
            Vector2 projectile2Location = centerOfScreen + normalizedMiddle * pointDistance;

            GraphicsDevice.SetRenderTarget(RenderTarget);

            _spriteBatch.Begin();
            _spriteBatch.Draw(Texture, targetBounds, Color.White);

            _spriteBatch.Draw(
                ProjectileLightBlue,
                compassIndicator,
                null,
                Color.White,
                MathHelper.ToRadians(octant * 45),
                new Vector2(0.5f),
                new Vector2(projectileSize * 3),
                SpriteEffects.None,
                0);

            foreach (Vector2 ploc in ProjectileLocations)
            {
                _spriteBatch.Draw(
                    Projectile, 
                    ploc,
                    null,
                    Color.White,
                    angle,
                    new Vector2(0.5f), 
                    new Vector2(projectileSize),
                    SpriteEffects.None,
                    0);
            }

            _spriteBatch.Draw(
                pointDistance >= lineLength ? ProjectileGreen : ProjectileRed,
                projectile2Location,
                null,
                Color.White * MathF.Sin(MathHelper.Pi * (float)(gameTime.TotalGameTime.TotalSeconds * 2 % 1)),
                angle,
                new Vector2(0.5f),
                new Vector2(projectileSize * 3),
                SpriteEffects.None,
                0);

            _spriteBatch.Draw(
                ProjectileBlue,
                relativeMouse,
                null,
                Color.White,
                angle,
                new Vector2(0.5f),
                new Vector2(projectileSize * 5),
                SpriteEffects.None,
                0);

            int textOffset = 0; 
            int textDistance = 12;
            _spriteBatch.DrawString(Font, $"Buffer: {backBounds.Width}/{backBounds.Height} ({backAspect.ToString("0.00")}) Target: {targetBounds.X}/{targetBounds.Y}/{targetBounds.Width}/{targetBounds.Height} ({targetAspect.ToString("0.00")})", new Vector2(3, 2 + textDistance * textOffset++), Color.Black);
            _spriteBatch.DrawString(Font, $"Mouse Raw: {MouseState.X}/{MouseState.Y}", new Vector2(3, 2 + textDistance * textOffset++), Color.Black);
            _spriteBatch.DrawString(Font, $"Mouse Offset: {mouseOffset.X.ToString("0.00")}/{mouseOffset.Y.ToString("0.00")}", new Vector2(3, 2 + textDistance * textOffset++), Color.Black);
            _spriteBatch.DrawString(Font, $"Mouse Relative: {relativeMouse.X.ToString("0000.00")}/{relativeMouse.Y.ToString("0000.00")} a: {MathHelper.ToDegrees(angle).ToString("000.00")} {directionalMessage}", new Vector2(3, 2 + textDistance * textOffset++), Color.Black);
            _spriteBatch.DrawString(Font, $"Center Normalized: {normalizedMiddle.X.ToString("0.00")}/{normalizedMiddle.Y.ToString("0.00")}", new Vector2(3, 2 + textDistance * textOffset++), Color.Black);
            _spriteBatch.DrawString(Font, $"Mouse Relative to Middle: {mouseFromMiddle.X.ToString("0.00")}/{mouseFromMiddle.Y.ToString("0.00")}", new Vector2(3, 2 + textDistance * textOffset++), Color.Black);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin();

            _spriteBatch.Draw(RenderTarget, new Rectangle((int)targetX, (int)targetY, (int)targetWidth, (int)targetHeight), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

    }

    enum CompassDir
    {
        W = 0,
        NW = 1,
        N = 2,
        NE = 3,
        E = 4,
        SE = 5,
        S = 6,
        SW = 7
    }
}
