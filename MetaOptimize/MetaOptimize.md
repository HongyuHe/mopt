# EncodingUtils.cs

## `LinearizeMultGenContinAndBinary()` methods

I think this is related to solving optimization problems using a solver, where the goal is to linearize the multiplication of variables, particularly a non-negative continuous variable with a binary variable. Linearization is necessary because standard optimization solvers, like those for linear programming, cannot handle multiplication of decision variables directly. Here's an explanation of the code in mathematical terms:

### 1. **Overview of the Functions:**

- The functions `LinearizeMultNonNegContinAndBinary` and `LinearizeMultTwoBinary` handle the multiplication of:
  - A non-negative continuous variable with a binary variable.
  - Two binary variables.

- The goal of these functions is to create a new variable (`output`) that represents the product of these variables in a way that can be handled by a linear solver.

### 2. **Linearization of Non-Negative Continuous and Binary Variables:**

Given a non-negative continuous variable \( x \) and a binary variable \( y \), the objective is to represent the product \( z = x \times y \) in a linear form. The challenge arises because \( x \times y \) is a non-linear expression, and linear solvers can't directly handle it.

#### **Mathematical Constraints:**

To represent \( z = x \times y \), the following linear constraints are introduced:

1. **Upper Bound Constraint:**
   \[
   z \leq M \times y
   \]
   Where \( M \) is a large constant (big M) that serves as an upper bound on the value of \( x \). If \( y = 0 \), this constraint forces \( z = 0 \). If \( y = 1 \), it allows \( z \) to be as large as \( x \) but not more than \( M \).

2. **Output Less than or Equal to Continuous Variable:**
   \[
   z \leq x
   \]
   This constraint ensures that \( z \) does not exceed \( x \) when \( y = 1 \).

3. **Lower Bound Constraint:**
   - If \( y = 1 \):
     \[
     z \geq x - M \times (1 - y)
     \]
     Since \( y = 1 \), this simplifies to \( z \geq x \), ensuring \( z = x \).

   - If \( y = 0 \):
     \[
     z \geq 0
     \]
     This forces \( z \) to be zero when \( y = 0 \).

### 3. **Handling the `notBinary` Parameter:**

The `notBinary` parameter introduces an additional complexity where instead of multiplying by \( y \), the code handles the multiplication by \( (1 - y) \). The constraints are adjusted accordingly:
- **Upper Bound Constraint:**
  \[
  z \leq M \times (1 - y)
  \]
  If \( y = 1 \), this forces \( z = 0 \), and if \( y = 0 \), it allows \( z \) to be as large as \( x \).

### 4. **Linearization of Two Binary Variables:**

Given two binary variables \( x \) and \( y \), the objective is to linearize their product \( z = x \times y \).

#### **Mathematical Constraints:**

1. **Upper Bound Constraint:**
   \[
   z \leq y \quad \text{or} \quad z \leq 1 - y
   \]
   Depending on whether the `notY` parameter is true or false, this constraint ensures that \( z \) respects the binary nature of \( y \).

2. **Upper Bound Based on First Binary Variable:**
   \[
   z \leq x
   \]
   This constraint ensures \( z \) respects the binary nature of \( x \).

3. **Lower Bound Constraint:**
   - If `notY = false`:
     \[
     z \geq x + y - 1
     \]
     This ensures that \( z = 1 \) only if both \( x = 1 \) and \( y = 1 \).

   - If `notY = true`:
     \[
     z \geq x - y
     \]
     This modifies the lower bound constraint when considering \( z = x \times (1 - y) \).

### **Upshot:**

The code seem to use a series of linear constraints to represent the product of continuous and binary variables, or two binary variables, within the framework of a linear optimization solver. The use of the big M constant is a common technique in linear programming to enforce these constraints, ensuring the solution space is properly bounded. The logical flow and conditional constraints adapt the formulation depending on whether the binary variable or its complement is used in the product.

## `LinearizeMultGenContinAndBinary()` methods

These functions build upon the previous functions, but it generalizes the linearization process to handle a general continuous variable (or polynomial) multiplied by a binary variable. This is more complex because the continuous variable is no longer restricted to be non-negative.

### 1. **Understanding the Generalization:**

- **General Continuous Variable:** Unlike non-negative continuous variables, a general continuous variable \( x \) can take both positive and negative values. This requires a more careful treatment to correctly linearize the multiplication with a binary variable \( y \).

- **Binary Variable:** \( y \) is still a binary variable, meaning it can take values of either 0 or 1.

### 2. **Mathematical Formulation:**

Given a general continuous variable \( x \) and a binary variable \( y \), the goal is to linearize the product \( z = x \times y \), where \( z \) is the output variable. The linearization must account for the possibility that \( x \) could be negative.

#### **Mathematical Constraints:**

To represent \( z = x \times y \), the following linear constraints are introduced:

1. **Lower Bound Constraint (when `notBinary = false`):**

   \[
   z \geq x - M \times (1 - y)
   \]
   This ensures:
   - When \( y = 1 \), \( z = x \).
   - When \( y = 0 \), \( z \geq x - M \), which forces \( z \) to be non-positive (but not necessarily 0 if \( x \) is negative).

   **Lower Bound Constraint (when `notBinary = true`):**
   
   \[
   z \geq x - M \times y
   \]
   This ensures:
   - When \( y = 0 \), \( z = x \).
   - When \( y = 1 \), \( z \geq x - M \), which forces \( z \) to be non-positive.

2. **Upper Bound Constraint (when `notBinary = false`):**

   \[
   z \leq x + M \times (1 - y)
   \]
   This ensures:
   - When \( y = 1 \), \( z = x \).
   - When \( y = 0 \), \( z \leq x + M \), allowing \( z \) to be zero or close to zero.

   **Upper Bound Constraint (when `notBinary = true`):**

   \[
   z \leq x + M \times y
   \]
   This ensures:
   - When \( y = 0 \), \( z = x \).
   - When \( y = 1 \), \( z \leq x + M \), allowing \( z \) to be zero or close to zero.

3. **Lower Bound Constraint (general form):**
   \[
   z \geq -M \times y
   \quad \text{or} \quad
   z \geq -M \times (1 - y)
   \]
   This constraint ensures that \( z \) respects the bound imposed by \( M \) depending on the value of \( y \).

4. **Upper Bound Constraint (general form):**
   \[
   z \leq M \times y
   \quad \text{or} \quad
   z \leq M \times (1 - y)
   \]
   This ensures that \( z \) does not exceed the bound imposed by \( M \) depending on the value of \( y \).

### 3. **Implementation Explanation:**

- **Create Variable:** `output` is the variable created to store the result of the multiplication \( z = x \times y \).

- **Add Constraints:** The function adds a series of linear constraints to the solver. These constraints are designed to ensure that the variable `output` correctly represents the product of the continuous variable and the binary variable under all circumstances.

- **Handling the `notBinary` Parameter:**
  - When `notBinary = false`, the code handles the multiplication as \( z = x \times y \).
  - When `notBinary = true`, the multiplication is handled as \( z = x \times (1 - y) \).

### 4. **Detailed Constraint Logic:**

The constraints are set up in pairs to cover both lower and upper bounds for the product:

- **First and Third Constraints:** These constraints ensure that the output is greater than or equal to the necessary bounds.
- **Second and Fourth Constraints:** These constraints ensure that the output is less than or equal to the necessary bounds.

The constraints are combined to enforce the linearization for both the cases of \( y \) being 0 or 1 (or \( 1 - y \) being 0 or 1, depending on the value of `notBinary`).

### **Upshot:**

This code linearizes the multiplication of a general continuous variable with a binary variable by introducing a set of linear constraints. These constraints make sure that the output variable accurately represents the product under all possible values of the binary variable and accounts for the possibility that the continuous variable could be negative.

