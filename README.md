# The Astro Spider

A small game about a spider who desperately wants to explore the space. 
His mission is simple, he has to visit 5 planets before he achieves his desire.
Although the task seems simple, navigating through space and landing on planets is not a simple task. It requires some very good manouvering skills.

Now comes your part! Of course a spider cannot manouver a rocket, so you'll have to do it. DO IT FOR THE SPIDER!

# Controls

`WASD / Left Thumb Stick` - turn the rocket

`Space / Right Trigger` - use the main thruster

`Mouse / Right Thumb Stick` - rotate the camera

`Left Mouse Button / Right Button` - shoot

# Technologies used

The game was made entierly using the Unity game engine. 

The packages we used are the new Input System for Unity and the Universal Rendering Pipeline package. Everything else was entierly written from scratch.

# AI

## Planet lander 

We used machine learning to train an agent to learn how to land on 5 planets. The goal of the agent was to learn how to land on planets with very little impact.

In order to train the AI we implemented the [Neuroevolution of Augmenting Topologies algorithm (NEAT)](http://nn.cs.utexas.edu/downloads/papers/stanley.ec02.pdf). We didn't use any existing implementation for the algorithm, it was written from scratch.

Finding the right fitness function was a rather hard task. We ended up with the following formula:
```f = s^e``` if the rocket was alive at the end of the epoch and it still had fuel and ```f = (s^e)/2``` otherwise.

The ```s``` term in the formula gives information about the jorney to the planets (how good the path was), while the ```e``` term gives information about how good the landings were. By default (if the planets don't move at all), both of them are equal to 1.

The ```s``` term is calculated using the following formula ```s = sum_i((d_i + d_max_i) * (3/2) - (f_i + df_i + df_max_i))```, where: 

* ```d_i``` is a number between 0 and 1, which represents how close the rocket got from its initial position to the planet at the end of the epoch / when the rocket landed on planet ```i```. It's used to reward the individuals who go in the right direction.

* ```d_max_i``` is the maximum value ```d_i``` had during the simulation. It's used to reward the individuals who go in the right direction.

* ```f_i``` is the amount of fuel consumed while going to planet ```i```. Used as punishment for individuals who use too much fuel.

* ```df_i``` is a number between 0 and 1, which represents how far the rocket got from it's initial position to a position that is farther away from the planet than the initial position. Its value is evaluated at the end of the epoch / when the rocket landed on planet ```i```. It's used to punish the individuals for not going in the right way.

* ```df_max_i``` is the maximum value ```df_i``` had during the simulation. It's used to punish the individuals for not going in the right way.

The ```e``` term is calculated using the following formula ```e = sum_i((l_dot_i * 2 + (1 - li_i + l_i * (1 - ili_i))) / 2^i)```, where:

*  ```l_dot_i``` is a number between 0 and 1, representing how good the angle between the rocket and the planet was during the landing. Has a value of 0 when the rocket crashes with the front face touching the planet and 1 when the landing angle is perfect. Every other value between 0 and 1 depends on the angle. It's used to reward the individuals for landing straight on the planet.

* ```ili_i``` is a number between 0 and 1, representing the landing speed. When the rocket lands with the maximum allowed speed or faster, this value is 1, otherwise it's smaller. It's used to punish the individuals for landing too fast.

* ```l_i``` has a value of 1 when the rocket successfully landed on planet ```i``` and 0 otherwise. It is used to give a big boost in fitness to those individuals who managed to land on planets. 

* ```li_i``` is the same as ```ili_i```, but with a minor difference. It gets to 1 slower than ```ili_i```, which means that it still has a value smaller than 1 when the rocket landed with a speed that is way above the maximum landing speed. This is useful when the rockets still haven't managed to land on a planet, but they managed to get to the planet.

At the end of this formula, we divide the result by ```2^i```, so that the exponent doesn't get too high when landing on multiple planets.

This formula is very rewarding for those individuals who manage to successfully land on planets, so there is a higher chance that their genes are preserved for the next generations.

We also implemented elitism to make sure that the good genes are carried on to the next generations (the top 3 individuals in the previous generation are inserted into the next generation twice). Even though we keep the best individuals, the best fitness may decrease from one generation to another. This is because the individuals don't perform 100% the same in each generation due to floating point precision problems and simulation speed problems (if the game lags, the simulation may be less accurate). One individual may even be able to land on a planet in one epoch, but not be able to do it again in the next generation, which may have a negative effect on the best fitness value.

Here are the charts for almost 80 epochs of training: 

<p>
  <img src="./Training%20Results/max_fitness.png" alt="best_fitness" style="width: 48%"/>
  <img src="./Training%20Results/average_fitness.png" alt="average_fitness" style="width: 48%"/>
</p>

These charts use a logarithmic scale, because the fitness values grow exponentially, so it wouldn't make sense to display them directly. As you can see, the charts are quite volatile, given that the fitness formula is exponential. Although volatile, it proved to be a good enough to train the rockets without human supervision in less than 100 epochs.

Even though the game is 3D, the AI for Planet Landing operates in 2D, as training it for 3D is much more complicated. We also removed the obstacles in the world while training the AI.

The individuals percieve the environment through a set of sensors. There are sensors surroundng the rockets, each giving 3 inputs for the neural networks:

* A number between 0 and 1, when the target planet is in sight of the sensor. The closer the planet is, the smaller this number is. When the target planet is not in sight, this input has a value of -1.

* A number between 0 and 1, when an obstacle is in sight of the sensor. The closer the obstacle is, the smaller this number is. When no obstacle is in sight, the input has a value of -1.

* A number between -1 and 1, representing the cos value of the angle between the sensor and the target planet (calculated using the dot product). Has a value of 1 when the sensor faces the direction of the planet and -1 when it faces the opposite direction. 

![sensors_gif](./Readme%20Resources/SensorsGif.gif)

Here's the AI landing on planets:

![watch_ai_gif](./Readme%20Resources/WatchAIGif.gif)

And here's how the training process looks like:

![train_ai_gif](./Readme%20Resources/TrainingGif.gif)

This scene does the training for the rockets by simulating every individual in the population in order to calculate their fitness scores. Each epoch has a timeout of ```2 * n``` minutes, where ```n``` is the index of the farthest planet a rocket has landed on. 

At the end of the epoch, you have the option to save the current training state as a `.json` file, or restore a previous training state by loading a different `.json` file. There is also a slider which controls the simulation speed, although, unless simulation is beng run on an extremely good computer, higher simulation speeds result in less accurate results and this may actually harm the training process.

## Boids

Besides this planet lander AI, we also implemented boids for the enemy spaceships in the "gamemode" where the rocket is being controlled by a human player.

![boids_gif](./Readme%20Resources/BoidsGif.gif)

They work by the standard rules for boids.

# Physical simulation & Animation

## Rocket movement

The Rocket contains a component called `RocketMovement`. This component is responsible for handling how the rocket reacts when the thrusters are being "accelerated". The rocket had 4 weak side thrusters and 1 big main thruster. The side thrusters are mainly used to tilt the rocket, while the big thruster is for going forward.

This component contains a method called `ApplyAcceleration()`, which takes as arguments the thruster that we wish to use and a number representing how "accelerated" the thruster is. The more accelerated it is, the more force it applies onto the rocket and the more fuel it consumes. This method is called by the `PlayerController` component, in the case of rockets that are controlled by a human player and it's called by the `NeuralNetworkController`, in the case of AI controlled agents.

![thrusters_gif](./Readme%20Resources/ThrustersGif.gif)

## Gravity

We also implemented custom gravity. Each planet has a gravity field. The gravity fields of each planets consist of two regions. One region, where the gravity force is a constant value (the yellow region) and one where the gravity force start so fade away until it reaches 0 (the blue region).

![fields_image](./Readme%20Resources/GravityFields.png)

We can have colliding gravity fields when two planets are too close to each other and the objects (asteroids or spaceships) between them have an accurate behaviour. We used Ruge-Kutta Order 4 to query the gravity force at one given point.

![asteroids_gif](./Readme%20Resources/AsteroidsGif.gif)

Although we implemented an accurate gravity simulation system, this made the game very hard, so, for the player rocket, we also added drag force so that it doesn't maintain its speed indefinitely. This made the game much more playable.

## Animation 

In addition to all of those physics and collisions systems, we also added animations. We used particle systems for the thrusters and for the rocket projectiles and we are playing an animation whenever the spider on top of the rocket shoots something. We also used particle systems when asteroids and spaceships are being shot.

![asteroids_gif](./Readme%20Resources/ShootGif.gif)

# Graphics programming

We've implemented multiple visual effects. 

The first one we implemented was a cartoonish shader. It makes the light fall onto surfaces with an equal intensity (a part of a surface is either lit or not lit, there is not much inbetween). Of course, there were some things that were added so it looks a little prettier, for example rim lighting. Our implementation is based on [this excellent article](http://vodacek.zvb.cz/archiv/750.html). The code is not identical as the synatx for writing shaders using URP (without using shadergraph) is not the same as the legacy syntax.

![toon_shdader](./Readme%20Resources/ToonShader.png)

On top of this shader, we've added a render pass that draws a black outline behind the objects being rendered. The way this shader work is by just scaling up the object a bit, making it black and rendering it behind the actual object. Our implementation is based on [this tutorial](https://learn.unity.com/tutorial/custom-render-passes-with-urp#5ff8d886edbc2a001fd0b4d0). Although the technique is not ideal and there are other better techniques for achieving the same effect, it seemed to be good enough for this project.

![outline_shader](./Readme%20Resources/OutlineShader.png)

The planets seemed a bit boring, that's why we created a new shader for them that just displaced the normals a little bit. This makes the surface seem a bit rough because of the lighting, even though the surface is flat. The way we displace the the normals is by using Simplex Noise (our implementation of Simplex noise was not written by us; you can find details about the author at the beginning of [the source file](./Assets/Scripts/Shaders/SimplexNoise.hlsl)). We combined noise values at different frequencies / amplitudes (we can call them octaves, as we iteratively multiply the frequency by 2, but decrease the amplitude by 2) to obtain some pleasing results.

![planet_shader](./Readme%20Resources/PlanetShader.png)

We've also added a bloom post-processing effect. The effect was implemented from scratch using render features (the only way to do post-processing while using the Scriptable Rendering Pipeline in Unity). The algorithm is the standard one:

Extract the bright parts of the image -> Perform Horizontal Blur on this image -> Perform Vertical Blur on this image -> Combine the blurred extract with the initial image to obtain the Bloom effect.

<p align="center">
  <img src="./Readme%20Resources/BloomRocket.png" alt="bloom_shader" style="width: 25%"/>
</p>


Another minor thing we implemented with regards to graphics programming are some of the UI elements. More specifically, the visualizer for the nerual network and the fitness evolution chart. The only way to create custom UI elements is by defining the geometry manually, so the vertices and indices for these elements are created and updated manually.

<p align="center" style="flex: 1; justify-content: space-evenly">
  <img src="./Readme%20Resources/NNDisplay.png" alt="nn_display" style="width: 23%; margin-right: 5%"/>
  &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
  <img src="./Readme%20Resources/FitnessChart.png" alt="fitness_display" style="width: 25%"/>
</p>
