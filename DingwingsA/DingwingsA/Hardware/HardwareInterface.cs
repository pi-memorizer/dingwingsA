using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            time = gameTime;
            core.run();
            base.Update(gameTime);
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

        public static BinaryReader getSaveReader(string path)
        {
#if WINDOWS
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
#endif
            return new BinaryReader(savegameStorage.OpenFile(path, FileMode.Open));
        }

        public static BinaryWriter getSaveWriter(string path)
        {
#if WINDOWS
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
#endif
            return new BinaryWriter(savegameStorage.OpenFile(path, FileMode.Create));
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
