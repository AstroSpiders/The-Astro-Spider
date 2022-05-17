# The Astro Spider

A small game about a spider who desperately wants to explore the space. 
His mission is simple, he has to visit 5 planets before he achieves his desire.
Although the task seems simple, navigating through space and landing on planets is not sa smple task. It requires some very good manouvering skills.

Now comes your part! Of course a spider cannot manouver a rocket, so you'll have to do it. DO IT FOR THE SPIDER!

# Controls: 

WASD/ Left Thumb Stick - turn the rocket

Space/ Right Trigger - use the main thruster

Mouse/ Right Thumb Stick - rotate the camera

Left Mouse Button/ Right Button - shoot

# Technologies used

The game was made entierly using the Unity game engine. 

The packages we used are the new Input System for Unity, and the Universal Rendering Pipeline package, everything else was entierly written from scratch.

# AI

We used machine learning to train an agent to learn how to land on 5 planets. The goal of the agent was to learn how to land on planets with very little impact.

In order to train the AI we implemented the Neuroevolution of Augmenting Topologies algorithm (NEAT). We didn't use any existing implementation for the algorithm, it was written from scratch.

Finding the right fitness function was a rather hard task. We ended up with the following formula:
```f=s^e``` if the rocket was alive at the end of the epoch and it still had fuel and ```f=(s^e)/2``` otherwise.

The ```s``` term in the formula gives information about the jorney to the planets (how good the path was), while the ```e``` term gives information about how good the landings were. By default (if the planets don't move at all), both of them are equal to 1.

The ```s``` term is calculated using the following formula ```s = sum_i((d_i + d_max_i) * (3/2) - (f_i + df_i + df_max_i))```.

Where: 

* ```d_i``` is a number between 0 and 1 which represents how close the rocket got from it's initial position to the planet at the end of the epoch/ when the rocket laned on planet ```i```. It's used to reward the individuals who go in the right direction.

* ```d_max_i``` is the maximum value ```d_i``` had during the simulation. It's used to reward the individuals who go in the right direction.

* ```f_i``` is the amount of fuel consumed while going to planet ```i```. Used as punishment for individuals who use too much fuel.

* ```df_i``` is a number between 0 and 1 which represents how far the rocket got from it's initial position to a position that is farther away from the planet than the initial position. It's value is evaluated at the end of the epoch/ when the rocket landed on planet ```i```. It's used to punish the individuals for not going in the right way.

* ```df_max_i``` is the maximum value ```df_i``` had during the simulation. It's used to punish the individuals for not going in the right way.

The ```e``` term is calculated using the following formula ```e = sum_i((l_dot_i * 2 + (1 - li_i + l_i * (1 - ili_i))) / 2^i)```.

Where:

*  ```l_dot_i``` is a number between 0 and 1 representing how good the angle between the rocket and the planet was during the 0. Has a value of 0 when the rocket crashes with the front face touching the planet, and 1 when the landing angle is perfect. And every other value between 0 and 1 depending on the angle. It's used to reward the individuals for landing straght on the planet.

* ```ili_i``` is a number between 0 and 1 representing the landing speed. When the rocket lands with the maximum allowed speed or faster, this value is 1, otherwise it's smaller. It's used to punish the individuals for landing too fast.

* ```l_i``` has a value of 1 when the rocket successfully landed on planet ```i``` and 0 otherwise. It is used to give a big boost in fitness to those individuals who managed to land on planets. 

* ```li_i``` same as ```ili_i```, but with a minor difference. It gets to 1 slower than ```ili_i```, which means that it still has a value smaller than 1 when the rocket landed with a speed that it's way above the maximum landing speed. This is useful when the rockets still haven't managed to land on a planet, but they managed to get to the planet.

At the end of this formula we divide the result by ```2^i``` so that the exponent doesn't get too high when landing on multiple planets.

This formula is very rewarding for those individuals who manage to successfully land on planets, so there is a higher chance that their genes are preserved for the next generations.

We also implemented elitism to make sure that the good genes are carried on to the next generations (the top 3 individuals in the previous generation are inserted into the next generation twice). Even though we keep the best individuals, the best fitness may decrease from one generation to another. This is because the individuals don't perform 100% the same in each generation due to floating point precision problems and simulation speed problems (if the game lags, the simulation may be less accurate). One individual may even be able to land on a planet in one epoch, but not be able to do it again in the next generation, which may have a negative effect on the best fitness value.

Here are the charts for almost 80 epochs of training: 

![best_fitness](https://github.com/AstroSpiders/The-Astro-Spider/blob/main/Training%20Results/max_fitness.png)
![average_fitness](https://github.com/AstroSpiders/The-Astro-Spider/blob/main/Training%20Results/average_fitness.png)

These charts use a logarithmic scale, because the fitness values grow exponentially, so it wouldn't make sense to display them directly. As you can see, the charts are quite volatile, given that the fitness formula is exponential. Although volatile, it proved to be a good enough to train the rockets withoug human supervision in less than 100 epochs.

Even though the game is 3D, the AI for Planet Landing operates in 2D, as training it for 3D was much more complicated. We also removed the obstacles in the world while training the AI.

The individuals percieve the environment through a set of sensors. There are sensors surroundng the rockets, each giving 3 inputs for the neural networks:

* A number between 0 and 1 when the target planet is in sight of the sensor. The closer the planet is, the smaller this number is. When the target planet is not in sight, this input has a value of -1.

* A number between 0 and 1 when an obstacle is in sight of the sensor. The closer the obstacle is, the smaller this number is. When no obstacle is in sight, the input has a value of -1.

* A number between -1 and 1 representing the cos value of the angle between the sensor and the target planet (calculated using the dot product). Has a value of 1 when the sensor faces the direction of the planet, and -1 when it faces the opposite direction. 

![sensors_gif](https://github.com/AstroSpiders/The-Astro-Spider/blob/main/Readme%20Resources/SensorsGif.gif)

Here's the AI landing on planets:

![watch_ai_gif](https://github.com/AstroSpiders/The-Astro-Spider/blob/main/Readme%20Resources/WatchAIGif.gif)

Besides this planet lander AI, we also implemented boids for the enemy spaceships in the "gamemode" where the rocket is being controlled by a human player.

![boids_gif](https://github.com/AstroSpiders/The-Astro-Spider/blob/main/Readme%20Resources/BoidsGif.gif)

They work by the standard rules for boids.

# Physical simulation & Animation

The Rocket contains a component called RocketMovement. This component is responsible for handling how the rocket reacts when the thrusters are being "accelerated". The rocket had 4 weak side thrusters and 1 big main thruster. The side thrusters are mainly used to tilt the rocket, while the big thruster is for going forward.

This component contains a method called ApplyAcceleration() which takes as arguments the thruster that we wish to use, and a number representing how "accelerated" the thruster is. The more accelerated it is, the more force it applies onto the rocket and the more fuel it consumes. This method is called by the PlayerController component in the case of rockets that are controlled by a human player, and it's called by the NeuralNetworkController in the case of AI controlled agents.

