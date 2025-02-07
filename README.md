# TileBlast

TileBlast is a puzzle game built in Unity that challenges players to match colored tiles and complete levels with a limited number of moves. This project demonstrates various game development techniques including grid management, event-driven programming, UI and audio management, animations using DOTween, and more.

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Project Structure](#project-structure)
- [Installation](#installation)
- [Usage](#usage)
- [Dependencies](#dependencies)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## Introduction

This project is a Unity-based tile puzzle game where players interact with a dynamically generated grid of tiles. Each level is configured using ScriptableObjects and is designed to provide a challenging yet engaging gameplay experience. The project leverages an event-driven architecture to keep various game components decoupled and maintainable.

This project was built for a case study and is not intended for commercial use.
## Features

- **Level Management:** Easily configure and load levels with custom settings using `LevelData` ScriptableObjects.
- **Grid & Tile System:** Dynamically generate a grid of tiles that react to user input and support group interactions.
- **Audio Management:** Manage background music and sound effects with volume controls and mute options.
- **UI Management:** Adaptive UI elements with smooth animations for score and movement updates.
- **Particle Effects:** Customizable particle effects triggered during tile interactions.
- **Event-driven Architecture:** Decoupled communication between game components using an event bus.
- **Utility Extensions:** Includes helpful utilities like list shuffling for randomizing elements.

## Project Structure

The project is organized into several namespaces:

- **Scripts.Level:** Contains level configuration and management scripts.
- **Scripts.Managers:** Manages various systems such as grid, audio, UI, and settings.
- **Scripts.Tiles:** Contains tile behavior scripts including initialization, interactions, and particle effects.
- **Scripts.Event:** Implements the event bus system for decoupled communication.
- **Scripts.Utilities:** Provides utility classes and extension methods, such as list shuffling and UI helpers.

## Installation

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/Rhalith/TileBlast.git
   ```

2. **Open in Unity:**
   - Launch Unity Hub.
   - Add the cloned repository to Unity Hub and open the project.
   - Use Unity 2020.3 LTS or later (recommended).

3. **Install Dependencies:**
   - This project uses [DOTween](http://dotween.demigiant.com/) for animations. You can import DOTween via the Asset Store or Package Manager.
   - Ensure that all required assets and packages are installed.

## Usage

**Play Mode:**

- Open the `LevelSelection` scene.
- Select Simulator and any mobile device.
- Press the Play button in the Unity Editor to start the game.

**Level Configuration:**

- Modify levels using the `LevelData` ScriptableObjects located in the `Assets/Resources/LevelData` folder.
- Adjust grid dimensions, move counts, target scores, and tile thresholds as needed.

**Audio & UI Settings:**

- Use the settings menu within the game to adjust audio volumes and toggle sound effects.
- UI animations for score and movement updates are automatically triggered via game events.

## Dependencies

- **Unity:** 2020.3 LTS or later (recommended)
- **DOTween:** For animations
- **Unity's built-in UI system**

## Contributing

Contributions are welcome! If you wish to improve the project, please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Commit your changes with clear descriptions.
4. Submit a pull request for review.

Please adhere to the project's coding conventions and ensure that your changes are well documented.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Contact

For any questions, suggestions, or issues, please contact [Nuh Yiğit Akman](mailto:akmannuhyigit@gmail.com).
