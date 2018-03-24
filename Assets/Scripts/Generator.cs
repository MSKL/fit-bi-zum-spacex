﻿using UnityEngine;
using System.Collections.Generic;

public class Generator : MonoBehaviourSingleton<Generator> {

    /*
     *  Generátor je generátor pro GA
     * 
     * Postup:
     *  1) vytvoř jednotlivce s random weights
     *  2) nech je žít a vyhodnoť jejich fitness
     *  3) vyhodnoť jejich fitness
     *  4) spař je podle fitness
     *  5) další generace
     * 
     */

    public GameObject entity;

    [Header("Genetic algorithm settings")]
    public int GA_NumOfEntitiesInGeneration;
    public float GA_MutationRateInPercent01 = 0.03F;

    /* Nastavení */
    public int brain_numOfInputs = 8;
    public int brain_numOfHiddenLayers = 6;
    public int brain_numOfNeuronsInHiddenLayer = 10;
    public int brain_numOfOutputs = 2;

    [Header("Seed settings")]
    public int globalSeed;
    int seedIterator = 0;

    [HideInInspector()]
    public List<GameObject> entityList;

    int tickCounter = 0;
    int generation = 0;
    public int entity_count = 0;

    public bool GeneratorEnabled = false;

    // Use this for initialization
    void Start () {
        entityList = new List<GameObject>();    // Inicializuj entityList
        entity_count = GA_NumOfEntitiesInGeneration;
	}

    void FixedUpdate() {
        if (Input.GetKeyDown(KeyCode.F)) {
            Enable();
        }

        if (GeneratorEnabled) {
            tickCounter++;

            if (tickCounter >= 600 || Input.GetKeyDown(KeyCode.N)) {
                transform.position = new Vector3(Random.Range(-13, 13), transform.position.y, transform.position.z);
                entity_count = GA_NumOfEntitiesInGeneration;
                CreateNextGenerationAndKillPrevious();
                tickCounter = 0;
                generation++;
                Debug.Log("Generation: " + generation);
            }
        }
    }

    public void Enable()
    {
        destroyAllEntities();
        GeneratorEnabled = true;
        globalSeed = System.DateTime.Now.GetHashCode();
        FirstGenerate();
    }

    public void Disable()
    {
        GeneratorEnabled = false;
        destroyAllEntities();
    }

    public void CreateNextGenerationAndKillPrevious()   // Tady probíhá iterace jednotlivých generací
    {
        // Get the average fitness
        double fitsum = 0;
        foreach (GameObject ent in entityList) {
            fitsum += ent.GetComponent<Handling>().fitness;
        }

        Drawing.Instance.values.Add((float)(fitsum / entityList.Count));
        Debug.Log("Average fitness: " + fitsum / entityList.Count);


        var newEntityBrainList = Genetic.ChildrenBrainList(Functions.EntitiesToBrainDictionary(entityList), GA_MutationRateInPercent01, globalSeed + seedIterator);

        destroyAllEntities();

        GenerateFromBrains(newEntityBrainList);
    }

    public void destroyAllEntities()
    {
        foreach (GameObject go in entityList)
        {
            go.GetComponent<Handling>().Deactivate();
            Destroy(go);
        }

        entityList.Clear();

    }

    public void FirstGenerate()
    {
        for (int x = 0; x < GA_NumOfEntitiesInGeneration; x++)
        {
            // Vector3 velocity = new Vector3(Random.Range(-3, 3), Random.Range(0, -3), 0);
            // Vector3 position = new Vector3(Random.Range(-12, 12), 12, 0);
            GameObject ga = Instantiate(entity, transform.position, Quaternion.Euler(0, 0, 0)) as GameObject;     // vygeneruj entity na start
            ga.transform.SetParent(transform);                                                                      // nastav parent transform (kvůli přehlednosti)

            Brain newBrain = new Brain(brain_numOfInputs, brain_numOfHiddenLayers, brain_numOfNeuronsInHiddenLayer, brain_numOfOutputs, globalSeed + seedIterator); // Inicializuj mozek

            ga.GetComponent<Handling>().Activate(globalSeed + seedIterator, newBrain);                            // aktivuj entitu

            entityList.Add(ga);                                                                                     // přidej do listu
            seedIterator++;
        }
    }

    public void GenerateFromBrains(List<Brain> _newBrains)
    {
        foreach(Brain br in _newBrains)
        {
            GameObject ga = Instantiate(entity, transform.position, Quaternion.Euler(0, 0, 0)) as GameObject;     // vygeneruj entity na start
            ga.transform.SetParent(transform);                                                                      // nastav parent transform (kvůli přehlednosti)

            ga.GetComponent<Handling>().Activate(globalSeed + seedIterator, br);                                  // aktivuj entitu

            entityList.Add(ga);                                                                                     // přidej do listu
            seedIterator++;

        }
    }
	
}
