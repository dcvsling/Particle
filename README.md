# Particle

MonoGame C# 粒子系統套件，提供靈活的粒子發射器與效果管線。

## 安裝

使用 NuGet 安裝套件：
```bash
dotnet add package Particle.System --version 1.0.0
```

## 快速開始

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particle;

// 建立發射器，每秒產生 20 顆粒子
var emitter = new Emitter<Particle>(
    () => new Particle(
        position: new Vector2(0,0),
        velocity: new Vector2(0, -100),
        acceleration: new Vector2(0, -9.8f),
        lifetime: 2f,
        color: Color.White),
    rate: 20f);

// 建立粒子系統並加入發射器
var system = new ParticleSystem();
system.AddEmitter(emitter);

// 更新與繪製，請在 MonoGame Game::Update / Draw 中呼叫：
system.Update(gameTime.ElapsedGameTime.TotalSeconds);
spriteBatch.Begin();
system.Draw(spriteBatch, yourTexture);
spriteBatch.End();
```

## 執行測試

```bash
dotnet test
```

## 打包

```bash
dotnet pack src/Particle/Particle.csproj -c Release -o ./nupkg
```

更多使用範例請參考 `Particle.Viewer` 專案。