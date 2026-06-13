# Guardião dos Cristais

Projeto acadêmico de plataforma 2D feito na Unity para a disciplina de Game Development da UNIFECAF.

## Descrição

O jogador controla Kael, um jovem guardião que precisa recuperar cristais mágicos espalhados pelo Reino de Eldoria. O jogo tem quatro fases com dificuldade progressiva, coletáveis, inimigos, HUD, plataformas móveis, escalada, sistema de vida e boss final.

## História

Os cristais de Eldoria mantinham o equilíbrio do reino, mas foram corrompidos por uma criatura ancestral. Kael atravessa campos, florestas, cavernas e o templo final para restaurar essa energia.

## Mecânicas

- Movimento 2D com Rigidbody2D.
- Andar, correr, pular e escalar.
- Cristais azuis e dourados.
- Pontuação e contador de cristais.
- Vida do jogador com invencibilidade temporária após dano.
- Slimes que patrulham e causam dano.
- Plataformas móveis.
- Boss final com 3 pontos de vida.
- Portal para troca de fase.
- Telas de menu, game over e vitória.

## Controles

| Ação | Tecla |
|---|---|
| Andar | A/D ou setas |
| Correr | Shift |
| Pular | Espaço |
| Escalar | W/S |
| Pausar | Esc |

## Estrutura

```text
Assets/
└── _Project/
    ├── Animations/
    ├── Audio/
    ├── Editor/
    ├── Prefabs/
    ├── Scenes/
    ├── Scripts/
    ├── Sprites/
    └── UI/
```

## Como abrir na Unity

1. Abra o Unity Hub.
2. Clique em `Add` ou `Adicionar projeto`.
3. Selecione a pasta deste repositório: `Game Develoment - Unifecaf`.
4. Abra com Unity `6000.0.77f1` ou uma versão compatível.
5. Se as cenas ainda não aparecerem, vá em `Tools > Guardiao dos Cristais > Gerar projeto completo`.
6. Abra `Assets/_Project/Scenes/MenuPrincipal.unity`.
7. Clique em `Play`.

## Como gerar o executável

1. Na Unity, abra `File > Build Profiles` ou `File > Build Settings`.
2. Confirme se estas cenas estão na lista, nesta ordem:
   - `MenuPrincipal`
   - `Fase01_Tutorial`
   - `Fase02_Floresta`
   - `Fase03_Caverna`
   - `Fase04_TemploBoss`
   - `GameOver`
   - `Vitoria`
3. Escolha a plataforma `Windows`.
4. Clique em `Build`.
5. Escolha uma pasta fora do projeto, por exemplo `Build/Windows`.
6. Depois da build, abra o arquivo `.exe` gerado para testar.

## Tecnologias

- Unity
- C#
- Rigidbody2D
- Collider2D
- Unity UI

## Assets usados

O projeto usa sprites, efeitos sonoros e músicas gratuitos da Kenney, baixados de OpenGameArt e Kenney.nl. Os arquivos originais ficam em `Assets/_Project/ExternalAssets`.
