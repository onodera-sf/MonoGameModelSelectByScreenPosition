using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ModelSelectByScreenPosition
{
	/// <summary>
	/// ゲームメインクラス
	/// </summary>
	public class Game1 : Game
	{
    /// <summary>
    /// グラフィックデバイス管理クラス
    /// </summary>
    private readonly GraphicsDeviceManager _graphics = null;

    /// <summary>
    /// スプライトのバッチ化クラス
    /// </summary>
    private SpriteBatch _spriteBatch = null;

    /// <summary>
    /// スプライトでテキストを描画するためのフォント
    /// </summary>
    private SpriteFont _font = null;

    /// <summary>
    /// モデル
    /// </summary>
    private Model _model = null;

    /// <summary>
    /// マーク
    /// </summary>
    private Texture2D _mark = null;

    /// <summary>
    /// マーク画像の中心位置
    /// </summary>
    private Vector2 _markCenterPosition = Vector2.Zero;

    /// <summary>
    /// マークの位置
    /// </summary>
    private Vector2 _markPosition = new Vector2(100.0f, 100.0f);

    /// <summary>
    /// モデルへの当たり判定フラグ
    /// </summary>
    private bool _isHit = false;

    /// <summary>
    /// カメラの位置
    /// </summary>
    private Vector3 _cameraPosition = new Vector3(0.0f, 0.0f, 10.0f);

    /// <summary>
    /// ビューマトリックス
    /// </summary>
    private Matrix _view;

    /// <summary>
    /// プロジェクションマトリックス
    /// </summary>
    private Matrix _projection;


    /// <summary>
    /// GameMain コンストラクタ
    /// </summary>
    public Game1()
    {
      // グラフィックデバイス管理クラスの作成
      _graphics = new GraphicsDeviceManager(this);

      // ゲームコンテンツのルートディレクトリを設定
      Content.RootDirectory = "Content";

      // マウスカーソルを表示
      IsMouseVisible = true;
    }

    /// <summary>
    /// ゲームが始まる前の初期化処理を行うメソッド
    /// グラフィック以外のデータの読み込み、コンポーネントの初期化を行う
    /// </summary>
    protected override void Initialize()
    {
      // ビューマトリックス
      _view = Matrix.CreateLookAt(
                  _cameraPosition,
                  Vector3.Zero,
                  Vector3.Up
              );

      // プロジェクションマトリックス
      _projection = Matrix.CreatePerspectiveFieldOfView(
                  MathHelper.ToRadians(45.0f),
                  (float)GraphicsDevice.Viewport.Width /
                      (float)GraphicsDevice.Viewport.Height,
                  1.0f,
                  100.0f
              );

      // コンポーネントの初期化などを行います
      base.Initialize();
    }

    /// <summary>
    /// ゲームが始まるときに一回だけ呼ばれ
    /// すべてのゲームコンテンツを読み込みます
    /// </summary>
    protected override void LoadContent()
    {
      // テクスチャーを描画するためのスプライトバッチクラスを作成します
      _spriteBatch = new SpriteBatch(GraphicsDevice);

      // フォントをコンテンツパイプラインから読み込む
      _font = Content.Load<SpriteFont>("Font");

      // モデルを作成
      _model = Content.Load<Model>("Model");

      // ライトとビュー、プロジェクションはあらかじめ設定しておく
      foreach (ModelMesh mesh in _model.Meshes)
      {
        foreach (BasicEffect effect in mesh.Effects)
        {
          // デフォルトのライト適用
          effect.EnableDefaultLighting();

          // ビューマトリックスをあらかじめ設定
          effect.View = _view;

          // プロジェクションマトリックスをあらかじめ設定
          effect.Projection = _projection;
        }
      }

      // マーク作成
      _mark = Content.Load<Texture2D>("Mark");

      // マークの中心位置
      _markCenterPosition = new Vector2(_mark.Width / 2, _mark.Height / 2);
    }

    /// <summary>
    /// ゲームが終了するときに一回だけ呼ばれ
    /// すべてのゲームコンテンツをアンロードします
    /// </summary>
    protected override void UnloadContent()
    {
      // TODO: ContentManager で管理されていないコンテンツを
      //       ここでアンロードしてください
    }

    /// <summary>
    /// 描画以外のデータ更新等の処理を行うメソッド
    /// 主に入力処理、衝突判定などの物理計算、オーディオの再生など
    /// </summary>
    /// <param name="gameTime">このメソッドが呼ばれたときのゲーム時間</param>
    protected override void Update(GameTime gameTime)
    {
      // キーボードの情報取得
      KeyboardState keyboardState = Keyboard.GetState();

      // ゲームパッドの情報取得
      GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

      // ゲームパッドの Back ボタン、またはキーボードの Esc キーを押したときにゲームを終了させます
      if (gamePadState.Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
      {
        Exit();
      }

      // 移動スピード
      float speed = 200.0f;

      // キーボードによるマークの移動
      if (keyboardState.IsKeyDown(Keys.Left))
      {
        _markPosition.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
      }
      if (keyboardState.IsKeyDown(Keys.Right))
      {
        _markPosition.X += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
      }
      if (keyboardState.IsKeyDown(Keys.Up))
      {
        _markPosition.Y -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
      }
      if (keyboardState.IsKeyDown(Keys.Down))
      {
        _markPosition.Y += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
      }

      // ゲームパッドによるマークの移動
      if (gamePadState.IsConnected)
      {
        _markPosition.X += gamePadState.ThumbSticks.Left.X * speed *
                               (float)gameTime.ElapsedGameTime.TotalSeconds;
        _markPosition.Y -= gamePadState.ThumbSticks.Left.Y * speed *
                               (float)gameTime.ElapsedGameTime.TotalSeconds;
      }

      // マウス処理
      MouseState mouseState = Mouse.GetState();

      if (mouseState.X >= 0 && mouseState.X < Window.ClientBounds.Width &&
          mouseState.Y >= 0 && mouseState.Y < Window.ClientBounds.Height &&
          mouseState.LeftButton == ButtonState.Pressed)
      {
        // マウスがウインドウ内にあればマウスの位置を優先する
        _markPosition = new Vector2(mouseState.X, mouseState.Y);
      }

      // ビューポートを取得
      Viewport viewport = GraphicsDevice.Viewport;

      // スクリーンの位置を Vector3 で作成 (Z は 1.0 以下を設定)
      Vector3 screenPosition = new Vector3(_markPosition, 1.0f);

      // スクリーン座標を３次元座標に変換
      Vector3 worldPoint = viewport.Unproject(screenPosition,
                                              _projection,
                                              _view,
                                              Matrix.Identity);

      // マークが指す方向へのレイを作成
      Ray ray = new Ray(_cameraPosition,
                        Vector3.Normalize(worldPoint - _cameraPosition));

      // 球とレイとの当たり判定を行う
      _isHit = false;
      foreach (ModelMesh mesh in _model.Meshes)
      {
        if (mesh.BoundingSphere.Intersects(ray) != null)
        {
          // 球とレイは交差している
          _isHit = true;
          break;
        }
      }

      // 登録された GameComponent を更新する
      base.Update(gameTime);
    }

    /// <summary>
    /// 描画処理を行うメソッド
    /// </summary>
    /// <param name="gameTime">このメソッドが呼ばれたときのゲーム時間</param>
    protected override void Draw(GameTime gameTime)
    {
      // 画面を指定した色でクリアします
      GraphicsDevice.Clear(Color.CornflowerBlue);

      // Zバッファを有効にする
      GraphicsDevice.DepthStencilState = DepthStencilState.Default;

      // モデルを描画
      foreach (ModelMesh mesh in _model.Meshes)
      {
        mesh.Draw();
      }

      // スプライトの描画準備
      _spriteBatch.Begin();

      // マーク描画
      _spriteBatch.Draw(_mark, _markPosition,
          null, Color.White, 0.0f,
          _markCenterPosition, 1.0f, SpriteEffects.None, 0.0f);

      // テキスト描画
      _spriteBatch.DrawString(_font,
          "Cursor Key Press or\r\n" + 
          "   MouseLeftButton Drag\r\n" + 
          "Hit : " + _isHit,
          new Vector2(50.0f, 50.0f), Color.White);

      // スプライトの一括描画
      _spriteBatch.End();

      // 登録された DrawableGameComponent を描画する
      base.Draw(gameTime);
    }
  }
}
