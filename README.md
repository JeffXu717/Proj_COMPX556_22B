# A c# console application for final project of COMPX556_22B
----
Both serial and parallel version of ACO(Ant colony optimiztion) have been implemented for MMDP (max-min diversity probelm)


## Command line options
### Generate Option
Generate ditance matrices as test instances  
command parameters: `gen -o 100 2 "D:\Temp\instances.csv"`  
Generate 2 distance matrices with n equal to 100, then write to file.

### Serial ACO Option
- command parameters:  ` sSolve "D:\Temp\instances.csv" 0.3 1000 20`  
Solve first matrix with m equal to 0.3n, max iteration 1000, 20 ants.  
sSolve [data set file] [m] [n] [number of ants]  

- command parameters:  ` sSolve "D:\Temp\instances.csv" 0.3 1000 20 -n 10`  
Solve first matrix 10 times with m equal to 0.3n, max iteration 1000, 20 ants.  
sSolve [data set file] [m] [n] [number of ants] -n [number of executions]  

- command parameters:  ` sSolve "D:\Temp\instances.csv" 0.3 1000 20 -a -n 10`  
Solve matrices 10 times with m equal to 0.3n, max iteration 1000, 20 ants.  
sSolve [data set file] [m] [n] [number of ants] -allMatrices -n [number of executions]  

- command parameters:  ` sSolve "D:\Temp\instances.csv" 0.3 1000 20 -a -n 10 -w "D:\Temp\result.txt"`  
Solve matrices 10 times with m equal to 0.3n, max iteration 1000, 20 ants. Write result to file.  
sSolve [data set file] [m] [n] [number of ants] -allMatrices -n [number of executions] -writeTo filepath

### Parallel ACO Option
- command parameters:  ` pSolve "D:\Temp\instances.csv" 0.3 1000 10 2`  
Solve first matrix with m equal to 0.3n, max iteration 1000, 2 colonies, each 10 ants.  
pSolve [data set file] [m] [n] [number of ants] [number of colonies]

- command parameters:  `pSolve "D:\Temp\instances.csv" 0.3 1000 10 2 -n 10`  
Solve first matrix 10 times with m equal to 0.3n, max iteration 1000, 2 colonies, each 10 ants.  
sSolve [data set file] [m] [n] [number of ants] -n [number of executions] [number of colonies] -n [number of executions]  

- command parameters:  ` pSolve "D:\Temp\instances.csv" 0.3 1000 10 2 -a -n 10`  
Solve matrices 10 times with m equal to 0.3n, max iteration 1000, 2 colonies, each 10 ants.
sSolve [data set file] [m] [n] [number of ants] [number of colonies] -allMatrices -n [number of executions]  

- command parameters:  ` pSolve "D:\Temp\instances.csv" 0.3 1000 10 2 -a -n 10 -w "D:\Temp\result.txt"`  
Solve matrices 10 times with m equal to 0.3n, max iteration 1000, 20 ants. Write result to file.
sSolve [data set file] [m] [n] [number of ants] [number of colonies] -allMatrices -n [number of executions] -writeTo filepath

