using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;

namespace Hardware
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class HardwareInterface : Game
    {
        public static GameTime time;
        public static HardwareInterface instance;
        public static Core core;
        static bool _f5, _f11;
        static int startingWidth, startingHeight;

        public HardwareInterface()
        {
            Graphics.graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            instance = this;
            time = new GameTime();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Window.AllowUserResizing = true;
            Graphics.graphicsDevice = GraphicsDevice;
            Graphics.init();
            Sound.init();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Graphics.loadContent(Content);
            Sound.loadContent(Content);
            core = new Core();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            Graphics.unloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Input.update();
            Sound.update();
            KeyboardState keyboardState = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
                Exit();
            if(keyboardState.IsKeyDown(Keys.F5)&&!_f5)
            {
                f5();
                //do other refreshing stuff here
            }
            if(keyboardState.IsKeyDown(Keys.F11)&&!_f11)
            {
                if(Graphics.graphics.IsFullScreen)
                {
                    Graphics.graphics.PreferredBackBufferWidth = startingWidth;
                    Graphics.graphics.PreferredBackBufferHeight = startingHeight;
                } else
                {
                    startingWidth = Graphics.graphics.PreferredBackBufferWidth;
                    startingHeight = Graphics.graphics.PreferredBackBufferHeight;
                    Graphics.graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    Graphics.graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                }
                Graphics.graphics.ToggleFullScreen();
            }

            _f11 = keyboardState.IsKeyDown(Keys.F11);
            _f5 = keyboardState.IsKeyDown(Keys.F5);

            time = gameTime;
            core.run();
            base.Update(gameTime);
        }

        public static void f5()
        {
            core = new Core();
            Core.animationMoney = 0;
            for (int i = 0; i < 3; i++) Core.adStates[i] = 0;
            Core.deadTime = 0;
            Core.exceptions.Clear();
            Core.money = 0;
            Core.newItemTime = 5;
            Core.p = new Player();
            Core.flags.Clear();

            ShopState.categories = new List<string>();
            ShopState.categoryIndex = 2;
            ShopState.topCategory = 0;
            ShopState.items = new List<List<ShopState.Item>>();
            ShopState.allItems = new Dictionary<string, ShopState.Item>();
            ShopState.itemIndex = ShopState.topItem = 0;
            ShopState.leftSide = true;
            ShopState.thingsBought = 0;

            Sound.setMusic(Sound.baseSong);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (time == null) return;
            Graphics.handleFrame(core);
            base.Draw(gameTime);
        }

        public static Stream getAssetStream(string path)
        {
            return TitleContainer.OpenStream(instance.Content.RootDirectory + path);
        }

        public static float deltaTime
        {
            get
            {
                return (float)time.ElapsedGameTime.TotalSeconds;
            }
        }

        public static float timeSinceLevelLoad
        {
            get
            {
                return (float)time.TotalGameTime.TotalSeconds;
            }
        }
    }
}
