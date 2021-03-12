# turkey-cannon

This is a 2D cannon shooting game where the player controls a cannon to shoot turkeys over a mountain.
The scene consists of a randomized (with midpoint-bisection) mountian in the middle, a cannon on the right, and some turkeys on the left.
Several factors affect the cannonball trajectory including gravity,
wind (the direction and magnitude of which are indicated by a cloud on the top) and collisions with scene objects.
Turkeys are modelled by [Verlet integration](https://en.wikipedia.org/wiki/Verlet_integration).
Turkeys slowly pace back and forth, and sometimes leap upward.
More detailed game design can be found [here](https://frankzhang427.github.io/pdf/turkey_cannon.pdf).
