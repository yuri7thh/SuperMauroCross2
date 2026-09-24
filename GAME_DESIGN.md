# Super Mauro Cross 2 — Manual de Design do Jogo

> **Propósito deste documento**: definir o que o jogo é, suas mecânicas e decisões de design.
> É a fonte da verdade do projeto. **Sempre que algo mudar (mecânica, configuração, decisão), atualize este arquivo.**

---

## 1. Visão geral

- **Gênero**: Infinite runner 2D side-scroller.
- **Premissa**: o jogador corre constantemente para a direita, desvia de obstáculos e usa uma mecânica de **parry** para defender-se de projéteis.
- **Engine**: Unity 6 (6000.3.10f1), URP 2D, pacote Input System.
- **Plataforma**: PC (teclado).
- **Estilo visual**: pixel art (por enquanto, placeholders substituíveis).
- **Morte**: 1 hit = morte (clássico de runner).

### Frase-resumo

> "Um runner lateral onde você pula obstáculos e, no ar, dá parry em projéteis que vêm da frente para acumular combo e multiplicar seu score."

---

## 2. Mecânicas principais

### 2.1 Corrida
- O jogador corre automaticamente para a direita.
- A velocidade base vem do sistema de dificuldade (aumenta com a distância).

### 2.2 Pulo
- Botão de pulo.
- Gravidade, altura e duração configuráveis (arco determinístico).
- Extras opcionais (toggleáveis): coyote time, input buffer e **jump cut** (pulo de altura variável — soltar cedo corta a subida).
- **Velocidade no ar**: toggle `slowInAir` deixa o jogador mais lento enquanto pulando, com multiplicador configurável (`airSpeedMultiplier`).

### 2.3 Parry (mecânica especial)
- **Só funciona no ar** (enquanto pulando).
- Projéteis vêm de fora da tela (lado direito) na direção do jogador.
- Apertar o botão de parry na janela exata neutraliza o projétil.
- **Janela de parry: GENEROSA** (default ~200 ms, configurável).
- **Implementação**: uma "bolha" de parry ao redor do jogador (raio generoso, default `parryRange = 3`), ativa **somente no ar**. Apertar parry com algum projétil dentro da bolha → parry com sucesso.
- **Efeito do parry**: destrói o projétil, `combo++`, multiplicador de score sobe, e o jogador dá um **dash até o alvo parryável** — percorre pelo menos até a posição dele (`dashDistance` = distância mínima, `dashDuration` = duração fixa), independente de segurar o botão.
- **Duas maneiras de dar parry**: **K** = parry — dash até o alvo; **L** = parry-pulo — dash até o alvo e **pula bem vertical** no final, com uma **desacelerada horizontal breve**. Configurável: `parryJumpVelocity` (força do pulo), `parryJumpVerticality` (0 = horizontal, 1 = 100% vertical), `parryJumpDeceleration` (desaceleração breve) e `parryJumpDecelerationTime`. Ambos one-shot.
- **Limite**: 1 parry por pulo, mas **acertar um parry devolve o parry** (pode parry de novo no mesmo ar) e **concede um pulo no ar** (double jump). Reset ao aterrissar.
- **Feedback**: slow motion + screen shake + partícula (+ som quando houver áudio).

### 2.4 Combo / Multiplicador de score
- `combo` = número de parries consecutivos.
- Multiplicador = `1 + combo * passo` (com teto configurável).
- **Combo reseta por tempo sem parry** (tempo configurável).

### 2.5 Morte
- Encostar em obstáculo → morte.
- Tomar projétil sem parry → morte.
- 1 hit = morte (sem barra de vida).

---

## 3. Elementos do jogo

### 3.1 Projéteis
- Vêm de fora da tela, do lado direito, em direção ao jogador.
- Altura com variação vertical aleatória (`heightVariance`).
- **Spawn por timer aleatório**: cada projétil nasce após um intervalo aleatório entre `spawnIntervalMin` e `spawnIntervalMax`, permitindo vários projéteis em tela ao mesmo tempo.
- **Velocidade sempre variada** dentro do range `speedMin`..`speedMax` (a cada spawn).
- **Distância de segurança vertical**: projéteis consecutivos nunca nascem em Y muito próximo (`minYSeparation`).
- Opções: linha reta fixa vs. mirar no jogador (toggleável).
- Usar object pooling.

### 3.2 Obstáculos (todos configuráveis/toggleáveis)
| Tipo | Descrição | Interação |
|------|-----------|-----------|
| Espinho (chão) | Área perigosa no chão | Pular por cima |
| Parede/bloco | Barreira | Pular por cima (ou destruir com dash) |
| Buraco/penhasco | Abertura no chão | Pular por cima |
| Inimigo de chão | Anda/corre em direção ao jogador | Só pular por cima |

> **Sprite / animação / hitbox por obstáculo** (`ObstacleConfig.asset`): cada tipo (Espinho, Parede, Inimigo) tem `sprite` + `spriteSize` (visual), animação (via `animatorController` completo ou um `idle` clip simples, ambos com prioridade sobre o sprite) e `hitboxShape`/`hitboxSize`/`hitboxOffset` (colisor, independente do visual). O contorno verde do hitbox é visível no Game/Scene view; `showHitboxes` (global) + `showHitbox` (por obstáculo) ligam/desligam. O Espinho usa `hitboxShape = Triangle` (base no chão) por padrão. `ObstacleIdle.anim` é o clip placeholder do controller base dos obstáculos.

---

## 4. Score

- **Score = distância percorrida + bônus de parry**, tudo multiplicado pelo combo/multiplicador.
- Recorde salvo (PlayerPrefs/JSON).

---

## 5. Dificuldade (baseado em pesquisa de runners)

Técnicas usadas por runners consagrados (Subway Surfers, Temple Run, Jetpack Joyride, Canabalt, Geometry Dash, Chrome Dino, Alto's Adventure):

1. **Escalonamento de velocidade**: sobe com o tempo/distância via curva **assintótica** (sobe rápido, desacelera até um teto) para nunca ficar injogável.
2. **Densidade de spawn / gap de segurança**: obstáculos por distância aumentam; o espaço livre entre eles diminui.
3. **Desbloqueio de tipos de obstáculo**: obstáculos mais difíceis só aparecem após um limiar de dificuldade.
4. **Complexidade de padrões**: no início obstáculos isolados; depois chunks pré-montados que combinam vários obstáculos.
5. **Frequência/velocidade de projéteis**: escalam com a dificuldade.
6. **Piso de tempo de reação**: nunca spawnar obstáculo impossível (manter reação mínima humana).
7. **Variável independente**: distância/score (não tempo).
8. **Escalar único de intensidade**: um valor `difficulty` (0..1) alimenta todos os subsistemas via curvas.

### Nossa implementação
- `DifficultyDriver` central: `difficulty = f(distância)` (curva editável).
- Dificuldade base no início configurável (`difficultyStartOffset`) + rampa (`difficultyRampDistance`).
- Esse valor mapeia, via **AnimationCurves individuais**, para: velocidade, intervalo de spawn, taxa de projétil, densidade, etc.
- Tudo editável no inspector, sem código.

---

## 6. Geração procedural do mapa (implementação atual)

- **Chão contínuo + buracos**: o `LevelGenerator` mantém um "cursor" (`_nextX`) e gera, à frente do jogador, segmentos de chão com tamanho aleatório, intercalados com buracos (gaps sem chão = penhascos). Nunca gera dois buracos seguidos, e a largada (25 unidades) é sempre chão limpo.
- **Obstáculos por peso**: em cada segmento de chão, há uma chance (da dificuldade) de spawnar 1 obstáculo, sorteado por peso entre uma lista de `ObstacleEntry` (template + peso + `minDifficulty` + toggle `enabled`). A posição é aleatória dentro do segmento; o obstáculo é "encaixado" no topo do chão.
- **Filtro por dificuldade**: cada `ObstacleEntry` tem `minDifficulty`; só entra no sorteio quando a dificuldade atual >= esse valor (desbloqueio progressivo de obstáculos).
- **Toggles por tipo**: `enabled` liga/desliga cada obstáculo individualmente.
- **Projéteis**: spawn independente (`ProjectileSpawner`), guiado por curva de intervalo da dificuldade.
- **Despawn**: objetos atrás do jogador são destruídos (chão e obstáculos). Pooling é um refinamento futuro.
- **Chunks (`ChunkLibrary`)**: o mapa é formado por chunks pré-autorados (assets) encaixados sequencialmente. Cada `ChunkData` tem `length`, `weight`, `minDifficulty`, toggle `enabled`, uma lista de `GroundSegment` (chão; buracos = espaços vazios entre eles) e uma lista de `ChunkObstacle` (posição relativa + tipo). O gerador sorteia chunks por peso e filtra por `minDifficulty`. O asset `ChunkLibrary` é criado em `Resources/Configs/` e editável no inspector.

> **Próximo passo (chunks completos)**: evoluir para "chunks" que combinem chão + buraco + obstáculos num único asset, permitindo sequências totalmente autoradas.

---

## 7. Arquitetura de código

### 7.1 Movimento: mundo vs jogador
- **Opção escolhida (A)**: jogador com velocidade constante à direita + câmera segue + gerador spawna à frente / despawna atrás (object pooling). Distância = deslocamento real.
- **Física**: Rigidbody2D **kinematic** (ou movimento manual) para pulo determinístico e parry preciso.

### 7.2 Decoupling
- Usar **ScriptableObject Events / UnityEvents** para que Player, Generator, Difficulty e UI conversem sem acoplamento forte.
- Facilita ligar/desligar partes pelo inspector.

### 7.3 Estrutura de pastas

```
Assets/
  Scripts/
    Core/          GameManager, PlaceholderFactory, CameraController, FeedbackManager, ParticleMover, ObjectPool, AudioManager
    Player/        PlayerController
    Obstacles/     Hazard, GroundEnemy
    Projectiles/   Projectile, ProjectileSpawner
    Generation/    LevelGenerator, ObstacleEntry, ChunkData, ChunkLibrary
    Background/    ParallaxLayer, ParallaxBackground
    Difficulty/    DifficultyDriver
    Data/          GameConfig, PlayerConfig, DifficultyProfile, ProjectileConfig (ScriptableObjects)
    Editor/        GameSetup, UISetup (assets idempotente + cena aditiva + UI autorada)
    UI/            UIManager
    Input/         InputReader
    GameBootstrap.cs  (fallback: monta a cena por código se não houver cena autorada)
  Resources/Configs/  (assets de config gerados automaticamente)
  Prefabs/         Player, Spike, Wall, GroundEnemy, Projectile (gerados automaticamente)
  Art/             White.png (sprite placeholder — substituir pela arte final)
  Scenes/          Game.unity (cena principal, autorada)
```

---

## 8. Configuração (o "tudo toggleável")

Toda a calibração mora em **ScriptableObjects** em `Assets/Resources/Configs/` (criados automaticamente pelo editor). Selecione o asset no Project window e edite no inspector — **as mudanças persistem**:

- **`GameConfig.asset`** — toggles globais (parry, combo, projéteis) + parâmetros de combo e score.
- **`PlayerConfig.asset`** — velocidade base (com `slowInAir`/`airSpeedMultiplier`), pulo (força/gravidade + jump cut), raio do parry, dash (distância/duração/direção/sustentação com `dashHoldMaxDuration` e `dashDecayCurve`), limites de câmera (`clampToCameraTop`/`cameraTopMargin`), killY.
- **`DifficultyProfile.asset`** — `AnimationCurves` de dificuldade, velocidade, intervalo de projéteis, chance de obstáculo e chance de buraco; `difficultyStartOffset` e `difficultyRampDistance`.
- **`ProjectileConfig.asset`** — velocidade (`speedMin`/`speedMax`), altura (com variação `heightVariance` e separação `minYSeparation`), distância e intervalo de spawn (`spawnIntervalMin`/`spawnIntervalMax`) dos projéteis.
- **`ObstacleConfig.asset`** — sprite (`sprite`/`spriteSize`), animação (`animatorController` completo ou `idle` clip, com prioridade) e hitbox (`hitboxShape`/`hitboxSize`/`hitboxOffset`) por obstáculo; `showHitboxes` (global) + `showHitbox` (por obstáculo) para visualizar o hitbox.

Cada componente tem um campo `config` (ex: `GameConfig` no `GameManager`). Se vazio, o componente carrega de `Resources/Configs/<nome>`; se não achar, usa os padrões em código. Assim dá para trocar presets inteiros ou sobrepor por asset.

**Campos que ainda ficam no inspector do componente** (podem virar configs no futuro):
- `LevelGenerator` — tamanhos de chão/buraco, cores, distâncias e lista de `ObstacleEntry` (peso, `minDifficulty`, toggle).
- `FeedbackManager` — slow motion, screen shake, partículas.
- `AudioManager` — sons, volume, toggles.

**Como editar**: no Project window, abra `Assets/Resources/Configs/`, clique no asset e ajuste (ex: desligar `parryEnabled` no `GameConfig`, arrastar as curvas do `DifficultyProfile`).

**Zero hardcode** de gameplay: mudar número = editar asset no inspector, não o script.

> **Próximo passo**: `LevelGenerator`/`Feedback`/`Audio` virarem configs e opções persistirem via PlayerPrefs.

---

## 9. UI / Telas

- Menu inicial.
- HUD: score, distância, combo/multiplicador.
- Game over + retry rápido.
- Tela de opções.
- Recorde salvo.

---

## 10. Decisões confirmadas

| Tema | Decisão |
|------|---------|
| Perspectiva | 2D side-scroller |
| Efeito do parry | Combo/multiplicador de score |
| Origem dos projéteis | Fora da tela (frente/direita) |
| Morte/dano | 1 hit = morte |
| Janela de parry | Generosa (~200 ms default, configurável) |
| Parry no chão? | Não, só no ar |
| Feedback de parry | Slow motion + screen shake + partícula |
| Reset do combo | Por tempo sem parry |
| Score | Distância + bônus de parry (× combo) |
| Obstáculos | Espinho, parede, buraco, inimigo de chão |
| Inimigo de chão | Só pular por cima |
| Dificuldade | DifficultyDriver com curvas (pesquisa acima) |
| Visual | Pixel art, placeholders por enquanto |
| Controles | Teclado (PC) |
| UI | Básico + tela de opções |
| Assets | Placeholders substituíveis |
| Engine | Unity 6 (6000.3.10f1) |

---

## 11. Roteiro de implementação

- [x] 1. Setup: cena base, input (pulo + parry), player placeholder correndo.
- [x] 2. Movimento + pulo (configuráveis).
- [x] 3. Obstáculos simples: espinho + colisão + morte.
- [x] 4. Geração: LevelGenerator + obstáculos por peso.
- [x] 5. Projéteis + Parry (janela, combo, feedback).
- [x] 6. Dificuldade: DifficultyDriver + curvas.
- [x] 7. Obstáculos restantes: parede, buraco, inimigo de chão.
- [ ] 8. ~~UI definitiva~~ **feita**: HUD (distância/score/combo), menu inicial, game over (score+recorde), tela de opções com toggles (parry/combo/projéteis/slow-mo/shake/partículas/som + volume) via `UIManager` (UGUI).
- [x] 8b. Opções persistentes via `PlayerPrefs` (toggles + volume).
- [x] 8c. Configuração via ScriptableObjects (GameConfig/PlayerConfig/DifficultyProfile/ProjectileConfig) em Resources/Configs.
- [x] 9. Polimento parcial: object pooling (projéteis/partículas) + sons procedurais + fix de slow-motion no game over.
- [x] 9b. Polimento parcial: coyote time + jump buffer, projéteis mais rápidos com a dificuldade, screen shake na morte.
- [ ] 9c. Polimento restante: partículas melhores, sons reais (assets), balanceamento fino.
- [x] 10. Sistema de padrões → evoluído para **chunks completos**: `ChunkLibrary` (chão + buraco + obstáculos num único asset, por peso/dificuldade).
- [x] 10b. Chunks completos (chão + buraco + obstáculos num único asset).
- [x] 11. Transição para prefabs + cena autorada: `GameSetup` (assets idempotente + `Ensure Game Scene` aditivo, sem sobrescrever).

---

## Como rodar

1. Abra o projeto na Unity 6 e deixe compilar (prefabs, configs e sprites placeholder são criados automaticamente — idempotente).
2. Se a cena `Game.unity` já existe, abra-a e dê **Play**. Se quiser montar uma cena nova do zero: **File → New Scene** → salve → **SMC → Ensure Game Scene** (adiciona só o que falta, sem sobrescrever).
3. Aperte **Play**.

**Controles**:
- **Espaço** = pular.
- **K** = parry (só no ar) → dash.
- **L** = parry-pulo (só no ar) → avança ao alvo e pula para cima.
- **R** (ou botão na tela) = reiniciar após o game over.

> `SMC → Ensure Game Scene` é **aditivo e idempotente**: verifica o que já existe e só cria/preenche o que está faltando (objetos, componentes, referências null). **Nunca apaga nem sobrescreve** suas alterações manuais. Pode rodar quantas vezes quiser.
>
> `GameBootstrap` (runtime) é só um fallback: monta a cena por código se ela não tiver um `LevelGenerator` (ex: abrir a `SampleScene`). Na cena autorada, tudo fica visível/editable e persiste.

---

## 12. Histórico de mudanças

| Data | Mudança |
|------|---------|
| 2026-08-23 | Criação do manual com o design inicial e decisões confirmadas. |
| 2026-08-23 | Implementado vertical slice jogável (core, player, obstáculos, projéteis, geração, dificuldade, HUD de debug, bootstrap). |
| 2026-08-23 | Adicionado object pooling (`ObjectPool`), sons procedurais (`AudioManager`) e correção do slow-motion no game over. |
| 2026-08-23 | UI definitiva em UGUI (`UIManager`): menu, HUD, game over e opções com toggles. |
| 2026-08-23 | Calibração migrada para ScriptableObjects (`GameConfig`, `PlayerConfig`, `DifficultyProfile`, `ProjectileConfig`) editáveis no inspector e persistentes. |
| 2026-08-23 | Sistema de padrões (`PatternLibrary`): padrões pré-definidos de obstáculos sorteados por peso/dificuldade. |
| 2026-08-23 | Geração migrada para **chunks completos** (`ChunkLibrary`/`ChunkData`): chão + buraco + obstáculos autorados num único asset. |
| 2026-08-23 | Polimento: coyote time + jump buffer (pulo responsivo), projéteis mais rápidos com a dificuldade, screen shake na morte. |
| 2026-08-23 | Indicador visual do parry (`ParryIndicator`): anel no ar (azul) que fica verde quando há projétil no alcance. |
| 2026-08-23 | Parry agora dá **dash** e é limitado a **1 por pulo**; corrigida a checagem de chão (ignora o próprio colisor). |
| 2026-08-23 | Suporte a animação do jogador (`PlayerAnimator`): expõe `IsGrounded`, `VelY`, `Speed` e trigger `Parry` ao Animator. |
| 2026-08-23 | Sistema de parallax (`ParallaxLayer`, camadas com velocidade ajustável) + chão customizável (`groundSprite`) + resolução padrão 1920x1080. |
| 2026-08-23 | Transição para prefabs + cena autorada (`GameSceneBuilder`): `Game.unity`, prefabs e sprite placeholder como assets reais. |
| 2026-08-23 | Setup não-destrutivo: `GameSetup` (assets idempotente + `Ensure Game Scene` aditivo). Removido o builder que sobrescrevia a cena. |
| 2026-08-23 | Parallax em 1:1 com o sprite (`scale` uniforme por camada, default 1); removidos `tileWidth`/`tileHeight`. |
| 2026-08-23 | UI migrada para Canvas autorado: `UIManager` virou controlador e `UISetup` constrói o Canvas na cena (editável no editor). |
| 2026-08-23 | Opções persistidas via `PlayerPrefs` (parry/combo/projéteis/slow-mo/shake/partículas/som + volume). |
| 2026-09-24 | Pulo de altura variável (jump cut), dash do parry com `dashDistance` e direção (frente/na direção do projétil), variação vertical dos projéteis (`heightVariance` > 0). |
| 2026-09-24 | Dash do parry sustentado (segurar o botão) com decaimento de força (`dashDecayCurve`) e tempo máximo (`dashHoldMaxDuration`); sprite rotaciona na direção do dash. |
| 2026-09-24 | Dash pausa ao tocar o chão ou o topo da câmera; jogador nunca sai do topo da tela (`clampToCameraTop`). |
| 2026-09-24 | Dificuldade com base inicial (`difficultyStartOffset`); rajadas de projéteis (`burstCount`/`burstVerticalSpread`) e variação de velocidade no spawn (`speedVariance`). |
| 2026-09-24 | Parede (obstáculo laranja) destruível ao ser atingida durante o dash (`Hazard.destructibleByDash`). |
| 2026-09-24 | Projéteis por timer aleatório (`spawnIntervalMin`/`spawnIntervalMax`) com velocidade sempre variada (`speedMin`/`speedMax`); parry bem-sucedido devolve o parry e concede pulo no ar. |
| 2026-09-24 | `ObstacleConfig`: sprite, hitbox (`hitboxSize`) e cor configuráveis por obstáculo no inspector. |
| 2026-09-24 | Obstáculos com sprite/animacão e hitbox independentes; formas de hitbox (`Box`/`Triangle`), animação com prioridade e visualização do hitbox (`showHitboxes`). |
| 2026-09-24 | Obstáculos aceitam `idle` clip (como o jogador), usando um `AnimatorOverrideController` sobre `ObstacleStateController`. |
| 2026-09-24 | Distância vertical de segurança entre projéteis consecutivos (`minYSeparation`). |
| 2026-09-24 | Corrigido obstáculo invisível: `ObstacleStateController` com Idle válido (`ObstacleIdle.anim`) + sprite de fallback; hitbox visível no Game view (LineRenderer) com toggle global e por obstáculo. |
| 2026-09-24 | Segunda forma de parry (botão L): parry-pulo — vai até o alvo parryável (teleporta) e pula para cima (`parryJumpVelocity`). |
| 2026-09-24 | Parry K agora também vai até o alvo (teleporta), one-shot — dash/sustentação removidos do fluxo de parry. |
| 2026-09-24 | Parry vira dash (não teleporte): percorre pelo menos até o alvo parryável (`dashDistance` mínimo + `dashDuration` fixo); L pula no final do dash. |
| 2026-09-24 | Pulo do L vertical com desaceleração breve configurável (`parryJumpVerticality`, `parryJumpDeceleration`, `parryJumpDecelerationTime`). |
