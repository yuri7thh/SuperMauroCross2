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
- Extras opcionais (toggleáveis): coyote time e input buffer.

### 2.3 Parry (mecânica especial)
- **Só funciona no ar** (enquanto pulando).
- Projéteis vêm de fora da tela (lado direito) na direção do jogador.
- Apertar o botão de parry na janela exata neutraliza o projétil.
- **Janela de parry: GENEROSA** (default ~200 ms, configurável).
- **Implementação**: uma "bolha" de parry ao redor do jogador (raio generoso, default `parryRange = 3`), ativa **somente no ar**. Apertar parry com algum projétil dentro da bolha → parry com sucesso.
- **Efeito do parry**: destrói o projétil, `combo++`, multiplicador de score sobe, e o jogador dá um **dash** (impulso horizontal rápido, configurável em `dashSpeed`/`dashDuration`).
- **Limite**: apenas **1 parry por pulo** — apertar o botão mais de uma vez no mesmo pulo não funciona (reset ao aterrissar).
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
- Altura e velocidade configuráveis.
- Opções: linha reta fixa vs. mirar no jogador (toggleável).
- Usar object pooling.

### 3.2 Obstáculos (todos configuráveis/toggleáveis)
| Tipo | Descrição | Interação |
|------|-----------|-----------|
| Espinho (chão) | Área perigosa no chão | Pular por cima |
| Parede/bloco | Barreira | Pular por cima |
| Buraco/penhasco | Abertura no chão | Pular por cima |
| Inimigo de chão | Anda/corre em direção ao jogador | Só pular por cima |

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
    Editor/        GameSetup (gera assets idempotente + cena aditiva)
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
- **`PlayerConfig.asset`** — velocidade base, pulo (força/gravidade), raio do parry, killY.
- **`DifficultyProfile.asset`** — `AnimationCurves` de dificuldade, velocidade, intervalo de projéteis, chance de obstáculo e chance de buraco.
- **`ProjectileConfig.asset`** — velocidade, altura, distância e intervalo dos projéteis.

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
- **K** = parry (só no ar).
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
| 2026-08-23 | Opções persistidas via `PlayerPrefs` (parry/combo/projéteis/slow-mo/shake/partículas/som + volume). |
