using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel.Design;
using System.Threading;

namespace Rendertarget_Playground
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        SpriteFont Font;
        Texture2D Texture;
        Texture2D Projectile;
        Texture2D Projectile2;
        RenderTarget2D RenderTarget;
        MouseState MouseState;

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
            RenderTarget = new RenderTarget2D(GraphicsDevice, 1024, 768);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("Font");
            Texture = Content.Load<Texture2D>("GameArea");

            Projectile = new Texture2D(GraphicsDevice, 1, 1);
            Projectile.SetData(new Color[] { Color.Orange });
            Projectile2 = new Texture2D(GraphicsDevice, 1, 1);
            Projectile2.SetData(new Color[] { Color.Red });
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState = Mouse.GetState();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Should probably only be done on resize
            _graphics.PreferredBackBufferWidth = GraphicsDevice.Viewport.Width;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.Viewport.Height;
            _graphics.ApplyChanges();

            GraphicsDevice.Clear(Color.CornflowerBlue);

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

            float relativeMouseX = 0;
            if (MouseState.X - targetX > 0)
                relativeMouseX = ((targetBounds.Width / (backBounds.Width - (2 * targetX)) * (MouseState.X - targetX)));
            if (relativeMouseX >= targetBounds.Width)
                relativeMouseX = targetBounds.Width;

            float relativeMouseY = 0;
            if (MouseState.Y - targetY > 0)
                relativeMouseY = ((targetBounds.Height / (backBounds.Height - (2 * targetY)) * (MouseState.Y - targetY)));
            if (relativeMouseY >= targetBounds.Height)
                relativeMouseY = targetBounds.Height;

            Vector2 centerOfScreen = targetBounds.Size.ToVector2() * 0.5f;
            Vector2 relativeMouse = new Vector2(relativeMouseX, relativeMouseY);
            Vector2 mouseFromMiddle = relativeMouse - centerOfScreen;

            Vector2 normalizedMiddle = mouseFromMiddle;
            normalizedMiddle.Normalize();

            Vector2[] projectileLocations = new Vector2[40];

            float tOffset = (float)(gameTime.TotalGameTime.TotalSeconds * 5 % 1) * (1f / projectileLocations.Length);
            for (int i = 0; i < projectileLocations.Length; i++)
            {
                projectileLocations[i] = centerOfScreen + (mouseFromMiddle * (1f / projectileLocations.Length * i + tOffset));
            }
            float projectileSize = 5;

            GraphicsDevice.SetRenderTarget(RenderTarget);

            _spriteBatch.Begin();
            _spriteBatch.Draw(Texture, targetBounds, Color.White);

            for (int i = 0; i < projectileLocations.Length; i++)
            {
                _spriteBatch.Draw(Projectile, new Rectangle((int)(projectileLocations[i].X - projectileSize * 0.5f), (int)(projectileLocations[i].Y - projectileSize * 0.5f), (int)projectileSize, (int)projectileSize), Color.White);
            }

            Vector2 projectile2Location = centerOfScreen - new Vector2(projectileSize) + normalizedMiddle * 200;
            _spriteBatch.Draw(Projectile2, new Rectangle((int)projectile2Location.X, (int)projectile2Location.Y, (int)(projectileSize * 2), (int)(projectileSize * 2)), Color.White);

            int textOffset = 0; 
            int textDistance = 18;
            _spriteBatch.DrawString(Font, $"Buffer: {backBounds.Width}/{backBounds.Height} ({backAspect.ToString("0.00")} Target: {targetBounds.X}/{targetBounds.Y}/{targetBounds.Width}/{targetBounds.Height} ({targetAspect.ToString("0.00")})", new Vector2(2, 2 + textDistance * textOffset++), Color.Black);
            _spriteBatch.DrawString(Font, $"Mouse Raw: {MouseState.X}/{MouseState.Y}", new Vector2(2, 2 + textDistance * textOffset++), Color.Black);
            _spriteBatch.DrawString(Font, $"Mouse Offset: {mouseOffset.X.ToString("0.00")}/{mouseOffset.Y.ToString("0.00")}", new Vector2(2, 2 + textDistance * textOffset++), Color.Black);
            _spriteBatch.DrawString(Font, $"Mouse Relative: {relativeMouse.X.ToString("0000.00")}/{relativeMouse.Y.ToString("0000.00")}", new Vector2(2, 2 + textDistance * textOffset++), Color.Black);
            _spriteBatch.DrawString(Font, $"Center Normalized: {normalizedMiddle.X.ToString("0.00")}/{normalizedMiddle.Y.ToString("0.00")}", new Vector2(2, 2 + textDistance * textOffset++), Color.Black);
            _spriteBatch.DrawString(Font, $"Mouse Relative to Middle: {mouseFromMiddle.X.ToString("0.00")}/{mouseFromMiddle.Y.ToString("0.00")}", new Vector2(2, 2 + textDistance * textOffset++), Color.Black);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin();

            _spriteBatch.Draw(RenderTarget, new Rectangle((int)targetX, (int)targetY, (int)targetWidth, (int)targetHeight), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
