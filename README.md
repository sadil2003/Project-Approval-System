
# 📌 PAS - Blind Matching Project Approval System

## 📖 Overview

The **Project Approval System (PAS)** is a secure web-based application designed to match student research project proposals with academic supervisors using a **Blind Matching mechanism**.

The system ensures fairness by hiding student identities during the initial selection phase and revealing them only after a supervisor confirms a match.

---

## 🎯 Objectives

* Enable students to submit research project proposals
* Allow supervisors to review projects anonymously
* Implement a **blind matching system** to reduce bias
* Provide an admin dashboard for system management

---

## 👥 User Roles

### 👨‍🎓 Student

* Register and login securely
* Submit project proposals (Title, Abstract, Tech Stack, Research Area)
* Edit or withdraw proposals
* Track project status (Pending / Matched)
* View supervisor details after matching

---

### 👨‍🏫 Supervisor

* Login securely
* Select expertise areas
* View anonymous project proposals
* Express interest and confirm matches
* View student details after matching

---

### 👨‍💼 Admin (Module Leader)

* Manage users (students & supervisors)
* Manage research areas
* Monitor all project allocations
* Reassign projects if needed

---

### ⚙️ System Administrator

* Configure ASP.NET Core environment
* Manage database and migrations
* Implement security (Role-Based Access Control)

---

## 🛠️ Technologies Used

* **Backend:** ASP.NET Core (C#)
* **Frontend:** Razor Pages / MVC
* **Database:** SQL Server
* **ORM:** Entity Framework Core
* **Version Control:** Git & GitHub

---

## 🔑 Key Features

* 🔐 Secure Authentication & Authorization
* 📂 Project Submission & Management
* 👀 Blind Review (No student identity shown)
* 🔄 Matching & Identity Reveal Mechanism
* 📊 Admin Dashboard
* ✅ Data Validation & Error Handling

---

## 🔄 System Workflow

1. Student submits a project proposal
2. Supervisor browses projects anonymously
3. Supervisor selects a project
4. System confirms match
5. Identity of both student and supervisor is revealed

---

## 🗄️ Database Design (Simplified)

### Entities:

* **User**

  * Id, Name, Email, Role

* **Project**

  * Id, Title, Abstract, TechStack, ResearchArea
  * Status (Pending / Matched)
  * StudentId, SupervisorId

---

## 🚀 Getting Started

### Prerequisites

* .NET SDK
* SQL Server
* Visual Studio / VS Code

---


---




