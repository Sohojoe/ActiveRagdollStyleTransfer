# Unity ML-Agents Python Interface and Trainers

The `mlagents` Python package is part of the
[ML-Agents Toolkit](https://github.com/Unity-Technologies/ml-agents).
`mlagents` provides a Python API that allows direct interaction with the Unity
game engine as well as a collection of trainers and algorithms to train agents
in Unity environments.

The `mlagents` Python package contains two sub packages:

* `mlagents.envs`: A low level API which allows you to interact directly with a
  Unity Environment. See
  [here](https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Python-API.md)
  for more information on using this package.

* `mlagents.trainers`: A set of Reinforcement Learning algorithms designed to be
  used with Unity environments. Access them using the: `mlagents-learn` access
  point. See
  [here](https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Training-ML-Agents.md)
  for more information on using this package.

## Installation

you cannot install the `mlagents` package in the traditional way, i.e.:

```sh
pip install mlagents
```

The reason for this is that this will install mlagents in version 0.8, and this project uses mlagents version 0.5
To circumvent this, enter in the ml-agents folder (where there is setup.py) and docs/Python-API


```sh
pip install -e .
```

This is enough to fix the version running (notice how the setup.py does not point to a generic commit, but rather to the specifici 0.5 version).


## Usage & More Information

For more detailed documentation, check out the
[ML-Agents Toolkit documentation.](https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Readme.md)
