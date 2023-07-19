# Online PacMan
![pacman](https://github.com/leticiap/Online-Pacman/assets/13660806/261fba56-313f-4727-a064-b7c88f2d63df)

Baseado no jogo PacMan, esse projeto foi desenvolvido como projeto para uma matéria que cursei durante o mestrado. A ideia principal do jogo é fugir dos fantasmas e comer todos os pellets. Existem pellets comuns, que não possuem nenhum efeito especial, e os super pellets, que dão um boost de velocidade para o jogador. O diferencial deste projeto é que dois jogadores aleatórios se conectam para competir quem consegue comer todos os pellets primeiro. Esta conexão online foi feita utilizando a biblioteca Photon.

## Cenas:
- Start.unity: menu principal, no qual o jogador pode colocar seu nome
- WaitingRoom.unity: sala de espera para encontrar outro jogador
- Game.unity: contém o jogo propriamente dito
- GameOver.unity: tela final com o nome do vencedor

## Código fonte:
- ConnectAndJoinRandom.cs: script para lidar com a conexão entre usuários.
- GameManager.cs: responsável por instanciar o mapa, os pellets, o jogador e os fantasmas. Também é responsável por saber e lidar com o estado atual do jogo.
- GameOverUI.cs: script para lidar com a UI da cena GameOver.
- GhostAI.cs: script com todo o comportamento dos fantasmas, que conta com duas estratégias diferentes:
  - Pinky: tentará prever onde o jogador está indo, e tentará encurralá-lo 4 espaços a frente.
  - Inky: calculará a distância do alvo do pinky com o seu alvo e tentará ir ao dobro desta distância.
- Grid.cs: script para definir a estrtura lógica do mapa.
- Launcher.cs: script para conectar o jogador ao servidor.
- Node.cs: script para auxiliar na navegação pelo mapa, tanto para jogadores quanto com informações para os fantasmas.
- OnJoinedInstantiate.cs: script para instanciar o mapa com todos os elementos para os jogadores, quando eles se conectarem.
- PlayerController.cs: script para lidar com o jogador, tanto como quesitos de controle, como quesitos de pontuação.
- PlayerNameInputField.cs: script para captar o nome do jogador na cena Start
- PlayerUI.cs: script que lida com a UI durante o jogo.
- Winner.cs: script usado para não destruir o objeto contendo o nome do vencedor, quando a cena for mudada (Game -> GameOver).
