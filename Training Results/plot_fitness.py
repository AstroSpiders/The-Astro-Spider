import json
import matplotlib.pyplot as plt

f = open('./5_planets_2D.json')
data = json.load(f)
max_fitnesses, average_fitnesses = [], []

for datapoint in data['EpochStats']:
    max_fitnesses += [datapoint['MaxFitness']]
    average_fitnesses += [datapoint['AverageFitness']]

f.close()

plt.plot(max_fitnesses)
plt.yscale("log")
plt.title('Max fitnesses')
plt.show()

plt.plot(average_fitnesses)
plt.yscale("log")
plt.title('Average fitnesses')
plt.show()
