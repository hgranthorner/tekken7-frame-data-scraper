import React, {useEffect, useState} from 'react';
import logo from './logo.svg';
import './App.css';

function App() {
    const [data, setData] = useState([])

    useEffect(() => {
        fetch("https://localhost:5001/api/characters")
            .then(d => d.json())
            .then(setData)
    }, [])
    return (
        <div className="App">
            <header className="App-header">
                <img src={logo} className="App-logo" alt="logo"/>
                <p>
                    Edit <code>src/App.js</code> and save to reload.
                </p>
                <a
                    className="App-link"
                    href="https://reactjs.org"
                    target="_blank"
                    rel="noopener noreferrer"
                >
                    Learn React
                </a>
            </header>
            <section>
                <ul>
                    {
                        Array.isArray(data) ? data.map((d, i) => (
                                <li key={i}>{d}</li>
                            ))
                            : data
                    }
                </ul>
            </section>
        </div>
    );
}

export default App;
