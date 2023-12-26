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

            GraphicsDevice.SetRenderTarget(RenderTarget);

            _spriteBatch.Begin();
            _spriteBatch.Draw(Texture, targetBounds, Color.White);
            _spriteBatch.DrawString(Font, $"Buffer: {backBounds.Width}/{backBounds.Height} ({backAspect.ToString("0.00")} Target: {targetBounds.X}/{targetBounds.Y}/{targetBounds.Width}/{targetBounds.Height} ({targetAspect.ToString("0.00")})", new Vector2(2, 2), Color.Black);
            _spriteBatch.DrawString(Font, $"Mouse Raw: {MouseState.X}/{MouseState.Y}", new Vector2(2, 16), Color.Black);
            _spriteBatch.DrawString(Font, $"Mouse Offset: {mouseOffset.X.ToString("0.00")}/{mouseOffset.Y.ToString("0.00")}", new Vector2(2, 30), Color.Black);
            _spriteBatch.DrawString(Font, $"Mouse Relative: {relativeMouseX.ToString("0000.00")}/{relativeMouseY.ToString("0000.00")}", new Vector2(2, 44), Color.Black);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin();

            _spriteBatch.Draw(RenderTarget, new Rectangle((int)targetX, (int)targetY, (int)targetWidth, (int)targetHeight), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
