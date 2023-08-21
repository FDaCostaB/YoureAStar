# CrowdSimulation- Group 7A - You're A Star
Assignements instructions : One of the most used algorithms in games is the A* algorithm. Even when you create an efficient implementation, applying this algorithm on a crowd of 100K+ agents to plan a global path in a big environment (of say 25 squared kilometers) may prove to be too costly. Be a star, and design and implement a variant that is fast enough to work in huge environments with huge crowds.

Comparison and visualisation of A*
Different map representation :
* Complete grid
* Subgoal graph
* Subgoal Graph Two-Level
* HPA
Different techniques :
* Constant heuristic
* Manhattan heuristic
* Euclidean heuristic
* Octile heuristic
* Chebyshev heuristic
* Combined with an adjustable heuristic constant multiplier
Different data structure :
* Unordered array
* Binary Heap
* Heap-on-Top(HOT) Queue

User Interface :
<img width="503" alt="UI2" src="https://github.com/FDaCostaB/YoureAStar/assets/47923208/d02c1021-a042-43e8-93a4-924efe5772cd">

Big environements - Amsterdam 5k by 5k grid map :
![Map-City](https://github.com/FDaCostaB/YoureAStar/assets/47923208/a9d5b98c-1344-4953-b148-7d707d0ce91f)

Implementation of HPA by Hugo Scurti: https://github.com/hugoscurti/hierarchical-pathfinding

Implementation of HOT Queue by BlueRaja: https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
