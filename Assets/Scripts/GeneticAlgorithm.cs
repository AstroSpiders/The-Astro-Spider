using System;
using System.Collections.Generic;
using System.Linq;

public class GeneticAlgorithm
{
    public enum NodeType
    {
        Sensor,
        Output,
        Hidden
    }

    public class Node
    {
        public NodeType NodeType { get; set; } = NodeType.Hidden;
        public int      Innov    { get; set; } = 0;

        public Node()
        {
        }

        public Node(Node other)
        {
            NodeType = other.NodeType;
            Innov    = other.Innov;
        }
    }

    public class Connect
    {
        public int   In      { get; set; } = 0;
        public int   Out     { get; set; } = 0;
        public float Weight  { get; set; } = 0.0f;
        public bool  Enabled { get; set; } = true;
        public int   Innov   { get; set; } = 0;

        public Connect()
        {
        }

        public Connect(Connect other)
        {
            In      = other.In;
            Out     = other.Out;
            Weight  = other.Weight;
            Enabled = other.Enabled;
            Innov   = other.Innov;
        }
    }

    public class Genome
    {
        public List<Node>    Nodes       { get; set; } = new List<Node>();
        public List<Connect> Connections { get; set; } = new List<Connect>();
        public float         Fitness     { get; set; } = 0.0f;

        public Genome()
        {
        }

        public Genome(Genome other)
        {
            Nodes       = new List<Node>();
            Connections = new List<Connect>();

            foreach (var node in other.Nodes)
                Nodes.Add(new Node(node));

            foreach (var connecton in other.Connections)
                Connections.Add(new Connect(connecton));

            Fitness = 0.0f;
        }

        public float CompatibilityDistance(float c1, float c2, float c3, float n, Genome other)
        {
            int   disjoint              = 0;
            int   excess                = 0;

            float totalDifference       = 0.0f;
            int   totalDifferentWeights = 0;
            
            int   thisIndex             = 0;
            int   otherIndex            = 0;

            while (thisIndex < Connections.Count && otherIndex < other.Connections.Count)
            {
                Connect thisConnect  = null;
                Connect otherConnect = null;

                if (thisIndex < Connections.Count)
                    thisConnect = Connections[thisIndex];
                if (otherIndex < other.Connections.Count)
                    otherConnect = other.Connections[otherIndex];

                if (thisConnect != null && otherConnect != null)
                {
                    if (thisConnect.Innov == otherConnect.Innov)
                    {
                        totalDifference += Math.Abs(thisConnect.Weight - otherConnect.Weight);
                        totalDifferentWeights++;

                        thisIndex++;
                        otherIndex++;
                    }
                    else
                    {
                        disjoint++;
                        if (thisConnect.Innov < otherConnect.Innov)
                            thisIndex++;
                        else
                            otherIndex++;
                    }
                }
                else
                {
                    excess++;

                    if (thisConnect != null)
                        thisIndex++;

                    if (otherConnect != null)
                        otherIndex++;
                }
            }

            float weightDiffAverage = 0.0f;
            if (totalDifferentWeights != 0)
                weightDiffAverage = totalDifference / totalDifferentWeights;

            return (c1 * excess / n) + (c2 * disjoint / n) + (c3 * weightDiffAverage);
        }
    }

    public class Specie
    {
        public List<Genome> Individuals                { get; set; } = new List<Genome>();
        public List<Genome> NewIndividuals             { get; set; } = new List<Genome>();
        public Genome       Representative             { get; set; } = null;
        public int          TargetOffspring            { get; set; } = 0;
        public float        TotalAdjustedFitness       { get; set; } = 0.0f;                     
        public float        MaxFitness                 { get; set; } = 0.0f;
        public int          GenerationsSinceMaxFitness { get; set; } = 0;

        public void UpdateRepresentative(Random random)
        {
            if (Individuals.Count != 0)
                Representative = Individuals[random.Next(Individuals.Count)];
            else
                Representative = null;
        }

        public void SwapGeneration(Random random)
        {
            Individuals    = NewIndividuals;
            NewIndividuals = new List<Genome>();
            UpdateRepresentative(random);
        }
    }

    public struct Parameters
    {
        public int   PopulationSize                   { get; set; }
        public int   EliteCount                       { get; set; }
        public int   EliteCopies                      { get; set; }
        public int   InNodes                          { get; set; }
        public int   OutNodes                         { get; set; }
        public float CrossoverDisableConnectionChance { get; set; }
        public float AddConnectionChance              { get; set; }
        public float AddNodeChance                    { get; set; }
        public float MutateWeightsChance              { get; set; }
        public float PerturbWeightChance              { get; set; }
        public float MaxWeightPerturbation            { get; set; }
        public float CompatibilityThreshold           { get; set; }
        public float InterspeciesMatingRate           { get; set; }
        public float PreviousGenMutatePercentage      { get; set; }
        public float C1                               { get; set; }
        public float C2                               { get; set; }
        public float C3                               { get; set; }
        public float N                                { get; set; }
        public int   GenerationsToStagnate            { get; set; }
    }

    public List<Specie> Population             { get; private set; } = new List<Specie>();
    public int          FirstOutputInnovNumber { get; private set; }

    private Parameters  _parameters;
    private int         _connectionInnovationNummber                 = 0;
    private int         _nodeInnovationNumber                        = 0;

    private Random      _random;

    public GeneticAlgorithm(Parameters parameters)
    {
        _random     = new Random();
        _parameters = parameters;

        Specie initialSpecie      = new Specie();
        initialSpecie.Individuals = new List<Genome>();

        for (int i = 0; i < _parameters.PopulationSize; i++)
        {
                _nodeInnovationNumber = 0;
            var individual            = new Genome();

            for (int j = 0; j < _parameters.InNodes; j++)
            {
                individual.Nodes.Add(new Node() { Innov = _nodeInnovationNumber, NodeType = NodeType.Sensor });
                _nodeInnovationNumber++;
            }
            for (int j = 0; j < _parameters.OutNodes; j++)
            {
                if (j == 0)
                    FirstOutputInnovNumber = _nodeInnovationNumber;

                individual.Nodes.Add(new Node() { Innov = _nodeInnovationNumber, NodeType = NodeType.Output });
                _nodeInnovationNumber++;
            }

            initialSpecie.Individuals.Add(individual);
        }

        initialSpecie.UpdateRepresentative(_random);
        Population.Add(initialSpecie);
    }

    public void Epoch()
    {
        UpdateStagnantSpecies();

        Dictionary<Tuple<int, int>, int> connectionsInnovationNumbers = new Dictionary<Tuple<int, int>, int>();
        Dictionary<Tuple<int, int>, int> nodesInnovationNumbers       = new Dictionary<Tuple<int, int>, int>();

        List<Genome> oldGenByFitness = new List<Genome>();
        
        foreach (var specie in Population)
            foreach (var individual in specie.Individuals)
                oldGenByFitness.Add(individual);

        oldGenByFitness = oldGenByFitness.OrderByDescending(genome => genome.Fitness).ToList();

        int addedCount = 0;

        for (int i = 0; i < _parameters.EliteCopies; i++)
        {
            for (int j = 0; j < _parameters.EliteCount; j++)
            {
                if (addedCount < _parameters.PopulationSize)
                {
                    AddToPopulation(new Genome(oldGenByFitness[j]), true, oldGenByFitness[j].Fitness);
                    addedCount++;
                }
            }
        }

        int remaining = _parameters.PopulationSize - addedCount;

        int prevGenCount = (int)(remaining * _parameters.PreviousGenMutatePercentage);
        for (int i = 0; i < prevGenCount; i++)
        {
            if (addedCount < _parameters.PopulationSize)
            {
                Genome oldGenome = RouletteWheelSelection(Population);
                Genome newGenome = new Genome(oldGenome);

                Mutate(newGenome, connectionsInnovationNumbers, nodesInnovationNumbers);

                AddToPopulation(newGenome, true, oldGenome.Fitness);
                addedCount++;
            }
        }

        remaining = _parameters.PopulationSize - addedCount;

        AdjustFitness();
        AdjustTargetOffsprings(remaining);

        for (int i = 0; i < Population.Count; i++)
        {
            for (int j = 0; j < Population[i].TargetOffspring; j++)
            {
                if (addedCount < _parameters.PopulationSize)
                {
                    Genome mom = RouletteWheelSelection(new[] { Population[i] });
                    Genome dad = RouletteWheelSelection(_random.NextDouble() < _parameters.InterspeciesMatingRate ? Population : new List<Specie> { Population[i] });

                    Crossover(mom, dad, out Genome child);

                    Mutate(child, connectionsInnovationNumbers, nodesInnovationNumbers);

                    AddToPopulation(child);
                    addedCount++;
                }
            }
        }
        
        while (addedCount < _parameters.PopulationSize)
        {
            Genome mom = RouletteWheelSelection(Population);
            Genome dad = RouletteWheelSelection(Population);

            Crossover(mom, dad, out Genome child);

            Mutate(child, connectionsInnovationNumbers, nodesInnovationNumbers);

            AddToPopulation(child);
            addedCount++;
        }

        foreach (var specie in Population)
            specie.SwapGeneration(_random);

        Population.RemoveAll(specie => specie.Representative is null);
    }

    private void UpdateStagnantSpecies()
    {
        foreach (var specie in Population)
        {
            bool updatedMaxFitness = false;
            foreach (var individual in specie.Individuals)
            {
                if (individual.Fitness > specie.MaxFitness)
                {
                    specie.MaxFitness                 = individual.Fitness;
                    specie.GenerationsSinceMaxFitness = 0;
                    updatedMaxFitness                 = true;
                }
            }
            if (!updatedMaxFitness)
                specie.GenerationsSinceMaxFitness++;
        }

        Population.RemoveAll(specie => specie.GenerationsSinceMaxFitness >= _parameters.GenerationsToStagnate);
    }

    private void AdjustFitness()
    {
        foreach (var specie in Population)
        {
            specie.TotalAdjustedFitness = 0.0f;
            foreach (var individual in specie.Individuals)
            {
                AdjustFitness(individual);
                specie.TotalAdjustedFitness += individual.Fitness;
            }
        }
    }

    private void AdjustTargetOffsprings(int totalRemaining)
    {
        float totalFitness = Population.Select(specie => specie.TotalAdjustedFitness).Sum();
        foreach (var specie in Population)
            specie.TargetOffspring = (int)(totalRemaining * (specie.TotalAdjustedFitness / totalFitness)); 
    }

    private Genome RouletteWheelSelection(IEnumerable<Specie> selectionSpecies)
    {
        float totalFitness = 0.0f;
        
        foreach (var specie in selectionSpecies)
            foreach (var individual in specie.Individuals)
                totalFitness += individual.Fitness;

        float slice       = (float)_random.NextDouble() * totalFitness;
        float accumulated = 0.0f;

        foreach (var specie in selectionSpecies)
        {
            foreach (var individual in specie.Individuals)
            {
                accumulated += individual.Fitness;
                if (accumulated >= slice)
                    return individual;
            }
        }

        return null;
    }

    private void Crossover(Genome mom, Genome dad, out Genome child)
    {
        Dictionary<int, NodeType> childNodeTypes = new Dictionary<int, NodeType>();

                                  child          = new Genome();
        

        int                       momIndex       = 0;
        int                       dadIndex       = 0;

        while (momIndex < mom.Connections.Count || dadIndex < dad.Connections.Count)
        {
            Connect momConnect = null;
            Connect dadConnect = null;

            if (momIndex < mom.Connections.Count)
                momConnect = mom.Connections[momIndex];
            if (dadIndex < dad.Connections.Count)
                dadConnect = dad.Connections[dadIndex];

            if (momConnect != null && dadConnect != null)
            {
                if (momConnect.Innov == dadConnect.Innov)
                {
                    Connect childConnect;
                    if (_random.NextDouble() <= 0.5f)
                        childConnect = new Connect(momConnect);
                    else
                        childConnect = new Connect(dadConnect);

                    if (childConnect.Enabled)
                        if (!momConnect.Enabled || !dadConnect.Enabled)
                            if (_random.NextDouble() < _parameters.CrossoverDisableConnectionChance)
                                childConnect.Enabled = false;
                    
                    child.Connections.Add(childConnect);

                    momIndex++;
                    dadIndex++;
                }
                else
                {
                    int     minInnov = Math.Min(momConnect.Innov, dadConnect.Innov);
                    Connect fitGene  = mom.Fitness >= dad.Fitness ? momConnect : dadConnect;

                    if (fitGene.Innov == minInnov)
                        child.Connections.Add(new Connect(fitGene));

                    if (minInnov == momConnect.Innov)
                        momIndex++;
                    else
                        dadIndex++;
                }
            }
            else
            {
                if (momConnect != null)
                {
                    if (mom.Fitness >= dad.Fitness)
                        child.Connections.Add(new Connect(momConnect));

                    momIndex++;
                }

                if (dadConnect != null)
                {
                    if (dad.Fitness > mom.Fitness)
                        child.Connections.Add(new Connect(dadConnect));

                    dadIndex++;
                }
            }
        }

        foreach (var momNode in mom.Nodes)
            if (!childNodeTypes.ContainsKey(momNode.Innov))
                childNodeTypes[momNode.Innov] = momNode.NodeType;

        foreach (var dadNode in dad.Nodes)
            if (!childNodeTypes.ContainsKey(dadNode.Innov))
                childNodeTypes[dadNode.Innov] = dadNode.NodeType;

        foreach (var keyValue in childNodeTypes)
            child.Nodes.Add(new Node()
            {
                Innov    = keyValue.Key,
                NodeType = keyValue.Value
            });

        child.Nodes = child.Nodes.OrderBy(node => node.Innov).ToList();
    }

    private void Mutate(Genome                           individual,
                        Dictionary<Tuple<int, int>, int> connectionsInnovationNumbers,
                        Dictionary<Tuple<int, int>, int> nodesInnovationNumbers)
    {

        if (_random.NextDouble() <= _parameters.AddConnectionChance)
            AddConnection(individual, connectionsInnovationNumbers);

        if (_random.NextDouble() <= _parameters.AddNodeChance)
            AddNode(individual, connectionsInnovationNumbers, nodesInnovationNumbers);

        if (_random.NextDouble() <= _parameters.MutateWeightsChance)
            MutateWeights(individual);

        individual.Nodes       = individual.Nodes.OrderBy(node => node.Innov).ToList();
        individual.Connections = individual.Connections.OrderBy(connection => connection.Innov).ToList();
    }

    private void AddToPopulation(Genome individual, bool addInitial = false, float individualFitness = 0.0f)
    {
        bool added = false;
        
        foreach (var specie in Population)
        {
            if (specie.Representative.CompatibilityDistance(_parameters.C1, _parameters.C2, _parameters.C3, _parameters.N, individual) < _parameters.CompatibilityThreshold)
            {
                specie.NewIndividuals.Add(individual);
                added = true;
                break;
            }
        }

        if (!added)
        {
            Specie newSpecie = new Specie();

            if (addInitial)
            {
                Genome oldIndividual         = new Genome(individual);
                       oldIndividual.Fitness = individualFitness;

                newSpecie.Individuals.Add(oldIndividual);
            }

            newSpecie.NewIndividuals.Add(individual);
            newSpecie.Representative = individual;

            Population.Add(newSpecie);
        }
    }

    private void AdjustFitness(Genome individual)
    {
        float initialFitness = individual.Fitness;
        int   sharing        = 0;

        foreach (var specie in Population)
        {
            foreach (var other in specie.Individuals)
            {
                float distance = individual.CompatibilityDistance(_parameters.C1, _parameters.C2, _parameters.C3, _parameters.N, other);
                if (distance < _parameters.CompatibilityThreshold)
                    sharing++;
            }
        }

        individual.Fitness = initialFitness / sharing;
    }

    private void AddConnection(Genome individual, Dictionary<Tuple<int, int>, int> connectionsInnovationNumbers)
    {
        int index;
        do
        {
            index = _random.Next(individual.Nodes.Count);
        } while (individual.Nodes[index].NodeType == NodeType.Output);
        int inNode = individual.Nodes[index].Innov;

        var potentialOutNodes = PotentialOutNodes(individual, inNode);

        if (potentialOutNodes.Length <= 0)
            return;

        int innovationNumber;
        int outNode = potentialOutNodes[_random.Next(potentialOutNodes.Length)];
        Tuple<int, int> key = new Tuple<int, int>(inNode, outNode);

        if (connectionsInnovationNumbers.ContainsKey(key))
        {
            innovationNumber = connectionsInnovationNumbers[key];
        }
        else
        {
            innovationNumber = _connectionInnovationNummber;
            connectionsInnovationNumbers[key] = innovationNumber;
            _connectionInnovationNummber++;
        }

        Connect connection = new Connect()
        {
            In      = inNode,
            Out     = outNode,
            Weight  = (float)_random.NextDouble(),
            Enabled = true,
            Innov   = innovationNumber
        };

        individual.Connections.Add(connection);
    }

    private void AddNode(Genome                           individual,
                         Dictionary<Tuple<int, int>, int> connectionsInnovationNumbers,
                         Dictionary<Tuple<int, int>, int> nodesInnovationNumbers)
    {
        var enabledConnections = individual.Connections.Where(connection => connection.Enabled).ToArray();

        if (enabledConnections.Length == 0)
            return;

        int pickedIndex              = _random.Next(enabledConnections.Length);
        var pickedConnection         = enabledConnections[pickedIndex];
            pickedConnection.Enabled = false;

        Tuple<int, int> oldConnectionKey = new Tuple<int, int>(pickedConnection.In, pickedConnection.Out);

        int node;
        if (nodesInnovationNumbers.ContainsKey(oldConnectionKey))
        {
            node = nodesInnovationNumbers[oldConnectionKey];
        }
        else
        {
            node                                     = _nodeInnovationNumber;
            nodesInnovationNumbers[oldConnectionKey] = node;
            _nodeInnovationNumber++;
        }

        individual.Nodes.Add(new Node
        {
            Innov    = node,
            NodeType = NodeType.Hidden
        });

        Tuple<int, int> key1 = new Tuple<int, int>(pickedConnection.In, node);
        Tuple<int, int> key2 = new Tuple<int, int>(node,                pickedConnection.Out);

        int innov1;
        if (connectionsInnovationNumbers.ContainsKey(key1))
        {
            innov1 = connectionsInnovationNumbers[key1];
        }
        else
        {
            innov1                             = _connectionInnovationNummber;
            connectionsInnovationNumbers[key1] = innov1;
            _connectionInnovationNummber++;
        }

        int innov2;
        if (connectionsInnovationNumbers.ContainsKey(key2))
        {
            innov2 = connectionsInnovationNumbers[key2];
        }
        else
        {
            innov2                             = _connectionInnovationNummber;
            connectionsInnovationNumbers[key2] = innov2;
            _connectionInnovationNummber++;
        }

        Connect connection1 = new Connect()
        {
            In      = pickedConnection.In,
            Out     = node,
            Weight  = 1.0f,
            Enabled = true,
            Innov   = innov1
        };

        Connect connection2 = new Connect()
        {
            In      = node,
            Out     = pickedConnection.Out,
            Weight  = pickedConnection.Weight,
            Enabled = true,
            Innov   = innov2
        };

        individual.Connections.Add(connection1);
        individual.Connections.Add(connection2);
    }

    private void MutateWeights(Genome individual)
    {
        foreach (var connect in individual.Connections)
            if ((float)_random.NextDouble() < _parameters.PerturbWeightChance)
                connect.Weight += RandomClamped() * _parameters.MaxWeightPerturbation;
            else
                connect.Weight = (float)_random.NextDouble();
    }

    private int[] PotentialOutNodes(Genome individual, int inNode)
    {
        List<int> result = new List<int>();

        foreach (var node in individual.Nodes)
        {
            if (node.Innov == inNode)
                continue;

            if (node.NodeType == NodeType.Sensor)
                continue;

            if (!CanReach(individual, node.Innov, inNode))
            {
                bool valid = true;

                foreach (var connection in individual.Connections)
                {
                    if (connection.In == inNode && connection.Out == node.Innov)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                    result.Add(node.Innov);
            }

        }

        return result.ToArray();
    }

    bool CanReach(Genome individual, int outNode, int inNode)
    {
        HashSet<int> visited = new HashSet<int>();
        Queue<int> bfsQueue = new Queue<int>();

        bfsQueue.Enqueue(outNode);
        visited.Add(outNode);

        while (bfsQueue.Count > 0)
        {
            var currentNode = bfsQueue.Dequeue();

            if (currentNode == inNode)
                return true;

            foreach (var connection in individual.Connections)
            {
                if (connection.In == currentNode && !visited.Contains(connection.Out))
                {
                    visited.Add(connection.Out);
                    bfsQueue.Enqueue(connection.Out);
                }
            }
        }

        return false;
    }

    private float RandomClamped() => (float)_random.NextDouble() * 2.0f - 1.0f;
}