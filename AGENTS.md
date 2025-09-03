# AGENTS 參考

- ~/.codex/AGENTS.md

# 簡介

用 .NET C# MonoGame 建立一個粒子系統(Particle System)
包括 可供參考的 Library 與其 測試程式 與 可視化的應用程式

# 架構

- [x] sdk: dotnet C# .NET latest
- [x] framework: MonoGame
- [x] solution: Particle

# Codebase

- [x] src
  - [x] Particle
  - [x] GUI
- [] app
- [x] test
  - [x] Particle.Tests

# 專案描述

## GUI

此專案為利用 MonoGame 繪製 GUI 的套件

- [] 將 Particle.Viewer 中 GUI 的部分通用的部分移動到此專案
- [] 建立 GUI 基礎類型及其常用 GUI 控制項
- [] 建立 GUI 使用的事件基礎類型 與 常用事件預設加入到相關的控制項
- [] 建立以 ECS 為對接的功能加載接口
- [] 建立預設和可能可直接使用的 Theme 與 Font 選項

## Particle

此專案為粒子系統的主要套件, 包含粒子系統主要功能實作, 與建構方式

- [x] 定義 Particle 結構 (位置、速度、加速度、生命週期、顏色)
- [x] 實作 Emitter 類別 (粒子產生器，控制產生率與初始設定)
- [x] 實作 ParticleSystem 類別 (管理多個 Emitter，提供 Update/Draw)
- [x] 實現粒子更新邏輯 (位置、速度、顏色隨生命週期變化)
- [x] 不處理 ContentPipeline, 改為僅使用已經載入完成的 Texture, 讓讀取 content 行為切出去給外部完成
- [] 建立 IParticleEffect 介面
- [] 盡可能的在使用 Particle 結構的類別, 將其結構以泛型方式實現, 如必須確認結構內容時, 則使用預設的 Particle 並提供抽象介面以供擴展
- [] 對於粒子運行的效能進行優化, 優化的部分以內部類別或方法來實現, 優化的方式可以使用 Span 或是 DOT 之類的方法, 避免使用 Unsafe 相關功能


## Particle.Tests

此專案為 Particle 專案的單元測試與整合測試

- [x] Particle 類別單元測試 (位置、速度、生命週期行為)
- [x] Emitter 類別單元測試 (產生邏輯與參數驗證)
- [x] ParticleSystem 更新與渲染測試 (確保 Update/Draw 正常執行)
  未完全測試 Draw 渲染流程，需在支援 GraphicsDevice 的環境中補充測試
- [x] 整合測試 IParticleEffect 與 ParticleSystem
  尚未整合 IParticleEffect 至 ParticleSystem，後續新增效果管線支援後再撰寫測試

## app - Particle.Viewer

參考 Particle 專案並用她實作可視化且可即時調整的程式

- [x] 建立 MonoGame DesktopGL 專案 (Particle.Viewer)
- [x] 載入 Particle 套件
- [] 參考 GUI 套件並以此為基礎實作 UI 控制 (滑桿/按鈕) 即時調整 Emitter 參數ㄏ
- [] 顯示並切換不同 IParticleEffect 範例
- [] 支援相機縮放與平移操作
- [] 實作 ExplosionEffect (爆炸特效)
- [] 實作 FountainEffect (噴泉特效)
- [] 實作 ColorOverLifetime 模組 (隨生命週期改變顏色)

## Packaging

此專案將 Particle 封裝為 NuGet 套件以供重用

- [x] 在 Particle.csproj 中設定 Package metadata
- [x] 使用 dotnet pack 生成 NuGet 套件 (.nupkg)

## CI / 持續整合

透過 GitHub Actions 自動化建置、測試與發布

- [x] 建立 GitHub Actions workflow (ci.yml)
- [x] 支援 Windows/macOS/Linux 平台建置
- [x] 自動執行單元測試與整合測試
- [x] 自動打包並發布 NuGet 套件

## 文件

提供完整的專案說明與 API 文件

- [x] 撰寫 README.md (專案概述、如何使用)
- [x] 撰寫 API 文件 (XML comments -> Markdown)
- [] 撰寫 範例程式碼並加入 repository

## 未來擴充

- [] TrailEffect (粒子軌跡特效)
- [] GravityWellEffect (重力井特效)
- [] CollisionEffect (粒子碰撞特效)