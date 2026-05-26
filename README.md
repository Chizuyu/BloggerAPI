# BloggerAPI - Backend

BloggerAPI adalah backend service berbasis **ASP.NET Core .NET 10** yang dirancang sebagai bagian dari "Hero Project". Project ini bertujuan untuk melakukan *Reverse Engineering* / *Cloning* terhadap API LKS Wilayah lama agar kompatibel dengan aplikasi Android yang sudah ada, namun dibangun dengan standar industri modern.

## Tech Stack & Architecture

- **Framework:** .NET 10 (Latest)
- **Database:** SQLite (file-based: `Blogger.db`)
- **ORM:** Entity Framework Core
- **Security:** 
  - JWT Authentication (JSON Web Token)
  - BCrypt Password Hashing
- **Pattern:** Repository Pattern dengan Folder Structure (Data, Entities, DTOs, Repositories, Controllers)
- **Naming Convention:** Penggunaan **Guid** sebagai ID primer di semua tabel untuk keamanan dan skalabilitas.

## Features

- **Auth System:** Registrasi dan Login dengan JWT Token.
- **Post Management:** Full CRUD (Create, Read, Update, Delete) postingan.
- **Category System:** Pengelompokan postingan berdasarkan kategori.
- **File Uploads:** System upload Thumbnail, Post Images, dan User Photo ke server lokal.
- **Like Logic:** Fitur toggle like dengan validasi (User tidak bisa menyukai postingan sendiri).
- **Profile & Me:** Endpoint khusus untuk mengelola data user yang sedang login.

## Legacy Compatibility (Android Integration)

API ini dikonfigurasi secara khusus agar mendukung integrasi dengan aplikasi Android Kotlin (Jetpack Compose + Retrofit) versi lama:

1. **Filenames Only:** Field gambar di database menyimpan path lengkap, namun API hanya mengembalikan *nama file saja* (menggunakan `Path.GetFileName`) agar sesuai dengan logic Glide/Picasso di Android.
2. **Nested Objects:** Response Post menyertakan objek `User` dan `Category` secara utuh, bukan hanya ID.
3. **Password Dummy:** Endpoint profil mengembalikan field password (string kosong/dummy) untuk mencegah error pada mapping model Android.
4. **Cleartext Traffic:** Redirection HTTPS dimatikan untuk mempermudah komunikasi dengan Emulator Android (10.0.2.2) via HTTP.

## Database Schema

| Entity | Fields |
| :--- | :--- |
| **User** | Id, FirstName, LastName, Username, PasswordHash, DateOfBirth, JoinDate, Photo |
| **Category** | Id, Name |
| **Post** | Id, Title, Content, Thumbnail, ImageContent, CreatedAt, UpdatedAt, UserId, CategoryId |
| **PostLike** | PostId, UserId (Many-to-Many Relation) |
| **Comment** | Id, Content, CreatedAt, PostId, UserId |

## Getting Started

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Chizuyu/BloggerAPI.git
