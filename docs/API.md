# API 文件

以下為核心類別及其簡要說明：

## Particle
- `Vector2 Position`：粒子當前位置
- `Vector2 Velocity`：粒子當前速度
- `Vector2 Acceleration`：粒子加速度
- `float Lifetime`：粒子總生命週期（秒）
- `float Age`：粒子已存活時間（秒）
- `bool IsAlive`：粒子是否存活（Age < Lifetime）
- `float LifeRemaining`：剩餘生命時間（秒）
- `Color Color`：粒子顏色與透明度

## Emitter
- `float Rate`：每秒產生粒子數量
- `IEnumerable<Particle> Emit(float deltaTime)`：根據速率與時間回傳新粒子序列

## ParticleSystem
- `IReadOnlyCollection<Particle> Particles`：目前所有活躍粒子清單
- `void AddEmitter(Emitter emitter)`：加入一個發射器
- `void Update(float deltaTime)`：更新系統（新增、更新與移除粒子）
- `void Draw(SpriteBatch spriteBatch, Texture2D texture)`：繪製所有粒子