# ActiveRagdollStyleTransfer
Research into using mocap (and longer term video) as style reference for training Active Ragdolls / locomotion for Video Games

(using Unity ML_Agents + [MarathonEnvs](https://github.com/Unity-Technologies/marathon-envs))

----

#### Goals
* Train active ragdolls using style reference from MoCap / Videos
* Integrate with [ActiveRagdollAssaultCourse](https://github.com/Sohojoe/ActiveRagdollAssaultCourse) & [ActiveRagdollControllers](https://github.com/Sohojoe/ActiveRagdollControllers) 

----

#### Contributors
* Joe Booth ([SohoJoe](https://github.com/Sohojoe))

----

#### Download builds : [Releases](https://github.com/Sohojoe/ActiveRagdollStyleTransfer/releases/)

----

## StyleTransfer002 (In-Progress)

Running (002.114) |
--- | 
![StyleTransfer002.114](images/StyleTransfer002.114-running-32m.gif) | 

Walking (002.113) | Backflip (002.115) |
--- | ---- |
![StyleTransfer002.113](images/StyleTransfer002.113-walking-32m.gif) | ![StyleTransfer002.115](images/StyleTransfer002.115-backflip-48m.gif) 

* **Model:** MarathonMan (modified MarathonEnv.DeepMindHumanoid)
* **Animation:** Runningv2, Walking, Backflip
* **Hypostheis:** Implement basic style transfer from mo-cap using MarathonEnv model
* **Outcome:** Starting to work... needs more training
  * Initial was able to train walking but not running (16m steps / 3.2m observations)
  * Through tweaking model was able to train running (32m steps / 6.4m observations)
  * Still struggling to train backflip but looks like I need to train for longer (current example is 48m steps / 9.6m observations)
* **References:** 
  * Insperation: [DeepMimic: Example-Guided Deep Reinforcement Learning of Physics-Based Character Skills arXiv:1804.02717 [cs.GR]](https://arxiv.org/abs/1804.02717 
* **Raw Notes:**
  * step = 1 physics step (200 fps)
  * observation = 1 training observation (40 fps)
  * Needed to make lots of modifications to model to improve training performance
  * Added sensors to feet improved trainging
  * Tweaking joints improved training
  * Training time = ~7h for 16m steps (3.2m observations)
  * see [RawNotes.002](RawNotes.002.md) for details on each experiment



## StyleTransfer001
![StyleTransfer001](images/StyleTransfer001.98b-10m.gif)
* **Model:** U_Character_REFAvatar
* **Animation:** HumanoidWalk
* **Hypostheis:** Implement basic style transfer from mo-cap
* **Outcome:** FAIL
  * U_Character_REFAvatar + HumanoidWalk has an issue whereby the feet collide. The RL does get learn to avoid - but it feels that this is slowing it down
* **References:** 
  * Insperation: [DeepMimic: Example-Guided Deep Reinforcement Learning of Physics-Based Character Skills arXiv:1804.02717 [cs.GR]](https://arxiv.org/abs/1804.02717 
* **Raw Notes:**
  * Aug 27 2018: Migrate to new repro and tidy up code so to make open source

